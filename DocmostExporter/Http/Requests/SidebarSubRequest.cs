using System.Text.Json.Serialization;

namespace DocmostExporter.Http.Requests;

public class SidebarSubRequest
{
    [JsonPropertyName("spaceId")]
    public string SpaceId { get; set; }
    
    [JsonPropertyName("pageId")] public string PageId { get; set; }
    
    [JsonPropertyName("page")]
    public int Page { get; set; }
}