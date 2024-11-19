namespace DocmostExporter;

public class Configuration
{
    public string DocmostUrl { get; set; } = "docs.docmost.example";
    public string DocmostEmail { get; set; } = "admin@docmost.example";
    public string DocmostPassword { get; set; } = "s3cret";

    public string SpaceSlug { get; set; } = "general";
    public string Storage { get; set; } = "generated";

    public bool UseHttpProxy { get; set; } = false;
    public string ProxyUrl { get; set; } = "http://proxy.http.example:8080";
    public string ProxyUsername { get; set; } = "itsme";
    public string ProxyPassword { get; set; } = "letmein";
    
    
}