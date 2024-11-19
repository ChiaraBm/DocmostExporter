using System.Text.RegularExpressions;
using DocmostExporter.Http.Requests;
using DocmostExporter.Http.Responses;
using MoonCore.Helpers;

namespace DocmostExporter;

public class DocmostService
{
    private string AccessToken;

    private HttpApiClient ApiClient;

    public DocmostService(string baseUrl, HttpClientHandler? httpClientHandler = null)
    {
        HttpClient httpClient;

        if (httpClientHandler == null)
            httpClient = new();
        else
            httpClient = new HttpClient(httpClientHandler);

        httpClient.BaseAddress = new Uri(baseUrl);

        ApiClient = new(httpClient);

        ApiClient.OnConfigureRequest += message =>
        {
            if (string.IsNullOrEmpty(AccessToken))
                return Task.CompletedTask;

            message.Headers.Add("Authorization", $"Bearer {AccessToken}");

            return Task.CompletedTask;
        };
    }

    public async Task Login(string email, string password)
    {
        var response = await ApiClient.PostJson<BaseResponse<LoginResponse>>("api/auth/login", new LoginRequest()
        {
            Email = email,
            Password = password
        });

        response.HandleError();

        AccessToken = response.Data.Tokens.AccessToken;
    }

    public async Task<Stream> FetchAsset(string link)
     => await ApiClient.GetStream("api" + link);

    public async Task Export(string slug, string location)
    {
        var spaces = await LoadSpaces();
        var space = spaces.FirstOrDefault(x => x.Slug == slug);

        if (space == null)
            throw new ArgumentException("Invalid slug. Unable to find a space with this slug on the docmost app");

        var pages = await LoadSidebar(space.Id);

        // Prepare file system
        Directory.CreateDirectory(location);
        Directory.CreateDirectory(PathBuilder.Dir(location, "pages"));
        Directory.CreateDirectory(PathBuilder.Dir(location, "files"));

        foreach (var page in pages)
        {
            var stream = await ExportPage(page.Id);

            var path = PathBuilder.File(location, "pages", $"{page.Id}.md");
            var fs = File.OpenWrite(path);

            await stream.CopyToAsync(fs);
            await fs.FlushAsync();

            fs.Close();
            stream.Close();

            // Search markdown for links
            var text = await File.ReadAllTextAsync(path);
            var matches = Regex.Matches(
                text,
                @"!\[[^\]]*\]\((\/files\/([a-f0-9\-]{36})\/[^\/]+\.[a-zA-Z0-9]+(\?[^\/]*\d)?)\)",
                RegexOptions.Singleline
            );

            var links = matches
                .Select(x => x.Value)
                .Where(x => x.Contains("]("))
                .Select(x => x.Split("](")[1])
                .Select(x => x.TrimEnd(')'))
                .ToArray();

            foreach (var link in links)
            {
                var linkParts = link.Split("/");
                var uuid = linkParts[2];

                Directory.CreateDirectory(PathBuilder.Dir(location, "files", uuid));
                var localLinkPath = PathBuilder.File(location, "files", uuid, linkParts[3].Split("?")[0]);

                var linkStream = await FetchAsset(link);

                var linkFs = File.OpenWrite(localLinkPath);

                await linkStream.CopyToAsync(linkFs);
                await linkFs.FlushAsync();

                linkFs.Close();
                linkStream.Close();
            }
        }
    }

    public async Task<Stream> ExportPage(string pageId, string format = "markdown")
    {
        var stream = await ApiClient.PostStream("api/pages/export", new ExportRequest()
        {
            PageId = pageId,
            Format = format
        });

        return stream;
    }

    public async Task<SpaceResponse[]> LoadSpaces()
    {
        return await RetrieveAllItems(async page
            => await ApiClient.PostJson<BaseResponse<ItemsResponse<SpaceResponse>>>("api/spaces")
        );
    }

    public async Task<SidebarResponse[]> LoadSidebar(string spaceId)
    {
        var result = new List<SidebarResponse>();
        
        var firstLevelItems = await RetrieveAllItems(
            async page => await ApiClient.PostJson<BaseResponse<ItemsResponse<SidebarResponse>>>(
                "api/pages/sidebar-pages",
                new SidebarRequest()
                {
                    SpaceId = spaceId,
                    Page = page
                }
            )
        );

        result.AddRange(firstLevelItems);

        foreach (var firstLevelItem in firstLevelItems)
        {
            if(!firstLevelItem.HasChildren)
                continue;

            await RetrieveSidebarSubItems(firstLevelItem, spaceId, result);
        }

        return result.ToArray();
    }

    private async Task RetrieveSidebarSubItems(SidebarResponse root, string spaceId, List<SidebarResponse> result)
    {
        var items = await RetrieveAllItems(
            async page => await ApiClient.PostJson<BaseResponse<ItemsResponse<SidebarResponse>>>(
                "api/pages/sidebar-pages",
                new SidebarSubRequest()
                {
                    SpaceId = spaceId,
                    Page = page,
                    PageId = root.Id
                }
            )
        );

        result.AddRange(items);
        
        foreach (var item in items)
        {
            if(!item.HasChildren)
                continue;

            await RetrieveSidebarSubItems(item, spaceId, result);
        }
    }

    private async Task<T[]> RetrieveAllItems<T>(Func<int, Task<BaseResponse<ItemsResponse<T>>>> runRequest)
    {
        var page = 1;
        var items = new List<T>();

        while (true)
        {
            var response = await runRequest.Invoke(page);

            response.HandleError();

            items.AddRange(response.Data.Items);

            if (!response.Data.Meta.HasNextPage)
                break;

            page++;
        }

        return items.ToArray();
    }
}