using System.Text.RegularExpressions;
using DocmostExporter.Http.Responses;
using DocmostExporter.MkDocs.PostProcessors;
using Microsoft.Extensions.Logging;
using MoonCore.Helpers;

namespace DocmostExporter.MkDocs;

public class MkDocsExporter
{
    private readonly DocmostService DocmostService;
    private readonly ILogger Logger;

    private readonly IPostProcessor[] PostProcessors =
    [
        new AdmotionsPostProcessor()
    ];

    public MkDocsExporter(DocmostService docmostService, ILogger logger)
    {
        DocmostService = docmostService;
        Logger = logger;
    }

    public async Task Export(string slug, string location)
    {
        Logger.LogInformation("Exporting as mkdocs project to {location}", location);
        
        Logger.LogInformation("Searching spaces for '{name}'...", slug);
        var spaces = await DocmostService.LoadSpaces();
        var space = spaces.FirstOrDefault(x => x.Slug == slug);

        if (space == null)
            throw new ArgumentException("Invalid slug. Unable to find a space with this slug on the docmost app");
        
        Logger.LogInformation("Loading page structure from '{name}'...", space.Name);
        var pages = await DocmostService.LoadSidebar(space.Id);

        Logger.LogInformation("Creating base file structure");
        
        // Prepare file system
        Directory.CreateDirectory(location);
        Directory.CreateDirectory(PathBuilder.Dir(location, "docs"));
        Directory.CreateDirectory(PathBuilder.Dir(location, "docs", "pages"));
        Directory.CreateDirectory(PathBuilder.Dir(location, "docs", "files"));

        // Export pages and files
        foreach (var page in pages)
        {
            Logger.LogTrace("Fetching markdown...");
            var stream = await DocmostService.ExportPage(page.Id);

            var filename = PageToFileName(page);
            var path = PathBuilder.File(location, "docs", "pages", filename);
            var fs = File.OpenWrite(path);

            await stream.CopyToAsync(fs);
            await fs.FlushAsync();

            fs.Close();
            stream.Close();

            // Search markdown for links
            Logger.LogTrace("Resolving links...");
            
            var text = await File.ReadAllTextAsync(path);
            var matches = Regex.Matches(
                text,
                @"\[([^\]]*)\]\(\/files\/([a-f0-9\-]{36})\/([^\/]+\.[a-zA-Z0-9]+)(\?[^\/]*\d)?\)",
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

                Directory.CreateDirectory(PathBuilder.Dir(location, "docs", "files", uuid));
                var localLinkPath = PathBuilder.File(location, "docs", "files", uuid, linkParts[3].Split("?")[0]);

                var linkStream = await DocmostService.FetchAsset(link);

                var linkFs = File.OpenWrite(localLinkPath);

                await linkStream.CopyToAsync(linkFs);
                await linkFs.FlushAsync();

                linkFs.Close();
                linkStream.Close();
            }
            
            // Post process markdown file
            Logger.LogTrace("Post processing...");
            var finalText = text;
            
            foreach (var processor in PostProcessors)
                finalText = processor.Process(finalText);

            await File.WriteAllTextAsync(path, finalText);
            
            Logger.LogInformation("Exported {page}", filename);
        }
        
        Logger.LogInformation("Creating mkdocs.yml");
        
        // Create configuration file
        var lines = new List<string>();

        void AddLine(string line, int level)
        {
            var x = "";

            for (int i = 0; i < level; i++)
                x += "  ";

            x += line;
            
            lines.Add(x);
        }
        
        AddLine($"site_name: {space.Name}", 0);
        AddLine("nav:", 0);

        void HandlePage(SidebarResponse page, int level)
        {
            var fileName = PageToFileName(page);
            
            if (page.HasChildren)
            {
                if (page.Title.EndsWith("!"))
                {
                    var titleTrimmed = page.Title.TrimEnd('!');
                    AddLine($"- {titleTrimmed}:", level + 1);
                }
                else
                {
                    AddLine($"- {page.Title}:", level + 1);
                    AddLine($"- Overview: pages/{fileName}", level + 2);
                }

                var children = pages.Where(x => x.ParentPageId == page.Id);

                foreach (var child in children)
                {
                    HandlePage(child, level + 1);
                }
            }
            else
            {
                AddLine($"- {page.Title}: pages/{fileName}", level + 1);
            }
        }
        
        var rootPages = pages.Where(x => x.ParentPageId == null);
        
        foreach (var rootPage in rootPages)
            HandlePage(rootPage, 0);
        
        AddLine("theme:", 0);
        AddLine("name: material", 1);
        
        AddLine("features:", 1);
        AddLine("- navigation.indexes", 2);
        
        AddLine("palette:", 1);
        
        AddLine("- scheme: default", 2);
        AddLine("toggle:", 3);
        AddLine("icon: material/brightness-7", 4);
        AddLine("name: Switch to dark mode", 4);
        
        AddLine("- scheme: slate", 2);
        AddLine("toggle:", 3);
        AddLine("icon: material/brightness-4", 4);
        AddLine("name: Switch to light mode", 4);
        
        AddLine("markdown_extensions:", 0);
        AddLine("- admonition", 1);
        AddLine("- pymdownx.details", 1);
        AddLine("- pymdownx.superfences", 1);
        AddLine("- pymdownx.tasklist:", 1);
        AddLine("custom_checkbox: true", 3);

        await File.WriteAllLinesAsync(PathBuilder.File(location, "mkdocs.yml"), lines);
    }

    private string PageToFileName(SidebarResponse page)
    {
        var uuidFirst = page.Id.Split("-")[0];
        var titleFormated = page.Title
            .ToLower()
            .Replace(" ", "_")
            .Replace("!", "")
            .Replace("?", "")
            .Replace(".", "")
            .Replace(",", "");

        return $"{uuidFirst}-{titleFormated}.md";
    }
}