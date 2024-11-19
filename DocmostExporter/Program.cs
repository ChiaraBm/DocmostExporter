using System.Net;
using DocmostExporter;
using DocmostExporter.MkDocs;
using Microsoft.Extensions.Logging;
using MoonCore.Extensions;
using MoonCore.Services;

var config = new ConfigService<Configuration>("config.json").Get();

var loggerFactory = new LoggerFactory();

loggerFactory.AddMoonCore(configuration =>
{
    configuration.Console.Enable = true;
    configuration.Console.EnableAnsiMode = true;
});

var logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("Starting Docmost Exporter");

HttpClientHandler? handler = null;

if (config.UseHttpProxy)
{
    handler = new HttpClientHandler()
    {
        Proxy = new WebProxy(config.ProxyUrl)
        {
            Credentials = new NetworkCredential(config.ProxyUsername, config.ProxyPassword)
        }
    };
}

var docmostService = new DocmostService(config.DocmostUrl, handler);

await docmostService.Login(config.DocmostEmail, config.DocmostPassword);

//await docmostService.Export("general", "ExportTest");

var mkDocsExporter = new MkDocsExporter(docmostService, logger);

await mkDocsExporter.Export(config.SpaceSlug, config.Storage);

logger.LogInformation("Successfully exported :)");