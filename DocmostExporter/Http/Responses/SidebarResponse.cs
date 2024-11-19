using System.Text.Json.Serialization;

namespace DocmostExporter.Http.Responses;

public class SidebarResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("parentPageId")]
    public string? ParentPageId { get; set; }
    
    [JsonPropertyName("hasChildren")]
    public bool HasChildren { get; set; }
}