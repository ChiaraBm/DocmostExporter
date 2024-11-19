using System.Text.Json.Serialization;

namespace DocmostExporter.Http.Requests;

public class SidebarRequest
{
    [JsonPropertyName("spaceId")]
    public string SpaceId { get; set; }
    
    [JsonPropertyName("page")]
    public int Page { get; set; }
}