using System.Text.Json.Serialization;

namespace MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Phi3.ChatCompletionWithData;

internal sealed class ChatWithDataMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}
