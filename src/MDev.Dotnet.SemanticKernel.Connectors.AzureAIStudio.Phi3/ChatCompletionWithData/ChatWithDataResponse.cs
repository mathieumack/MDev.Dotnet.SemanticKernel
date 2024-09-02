using System.Text.Json.Serialization;

namespace MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Phi3.ChatCompletionWithData;

internal sealed class ChatWithDataResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public int Created { get; set; } = default;

    [JsonPropertyName("choices")]
    public IList<ChatWithDataChoice> Choices { get; set; } = Array.Empty<ChatWithDataChoice>();

    [JsonPropertyName("usage")]
    public ChatWithDataUsage Usage { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonConstructor]
    public ChatWithDataResponse(ChatWithDataUsage usage)
    {
        this.Usage = usage;
    }
}
