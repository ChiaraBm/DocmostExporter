using System.Text.Json.Serialization;

namespace DocmostExporter.Http.Responses;

public class ItemsResponse<T>
{
    [JsonPropertyName("items")]
    public T[] Items { get; set; }

    [JsonPropertyName("meta")]
    public MetaData Meta { get; set; }
    
    public class MetaData
    {
        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage { get; set; }
        
        [JsonPropertyName("hasPrevPage")]
        public bool HasPrevPage { get; set; }
        
        [JsonPropertyName("limit")]
        public int Limit { get; set; }
        
        [JsonPropertyName("page")]
        public int Page { get; set; }
    }
}