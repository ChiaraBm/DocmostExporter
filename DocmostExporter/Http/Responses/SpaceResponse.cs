using System.Text.Json.Serialization;

namespace DocmostExporter.Http.Responses;

public class SpaceResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("slug")]
    public string Slug { get; set; }
}