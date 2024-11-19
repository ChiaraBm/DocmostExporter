using System.Text.Json.Serialization;

namespace DocmostExporter.Http.Requests;

public class ExportRequest
{
    [JsonPropertyName("pageId")]
    public string PageId { get; set; }
    
    [JsonPropertyName("format")]
    public string Format { get; set; }
}