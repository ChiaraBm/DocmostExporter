using System.Text.Json.Serialization;
using MoonCore.Exceptions;

namespace DocmostExporter.Http.Responses;

public class BaseResponse<T>
{
    [JsonPropertyName("data")]
    public T Data { get; set; }

    [JsonPropertyName("status")]
    public int Status { get; set; }
    
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    public void HandleError()
    {
        if(Success)
            return;

        throw new HttpApiException("An error occured", Status);
    }
}