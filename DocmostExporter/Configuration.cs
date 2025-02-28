namespace DocmostExporter;

public class Configuration
{
    public string Url { get; set; } = "docs.docmost.example";
    public string Email { get; set; } = "admin@docmost.example";
    public string Password { get; set; } = "s3cret";

    public string SpaceSlug { get; set; } = "general";
    public string Storage { get; set; } = "generated";
}