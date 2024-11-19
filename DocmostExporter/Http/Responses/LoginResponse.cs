using System.Text.Json.Serialization;

namespace DocmostExporter.Http.Responses;

public class LoginResponse
{
    [JsonPropertyName("tokens")]
    public TokensData Tokens { get; set; }
    
    public class TokensData
    {
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }
        
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
    }
}