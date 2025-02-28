using System.Net;
using DocmostExporter;
using DocmostExporter.MkDocs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MoonCore.EnvConfiguration;
using MoonCore.Extensions;
using MoonCore.Helpers;
using MoonCore.Services;

var configurationBuilder = new ConfigurationBuilder();

configurationBuilder.AddJsonFile(
    PathBuilder.Dir(Directory.GetCurrentDirectory(), "config.json"),
    optional: true
);

configurationBuilder.AddEnvironmentVariables(prefix: "EXPORTER_", separator: "_");

var configurationRoot = configurationBuilder.Build();
var configuration = configurationRoot.Get<Configuration>()!;

var loggerFactory = new LoggerFactory();

loggerFactory.AddMoonCore();

var logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("Starting Docmost Exporter");
logger.LogInformation("URL: {url}", configuration.Url);

var docmostService = new DocmostService(configuration.Url);

await docmostService.Login(configuration.Email, configuration.Password);

//await docmostService.Export("general", "ExportTest");

var mkDocsExporter = new MkDocsExporter(docmostService, logger);

await mkDocsExporter.Export(configuration.SpaceSlug, configuration.Storage);

logger.LogInformation("Successfully exported :)");