using System.Text.Json.Serialization;

namespace MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Phi3.ChatCompletionWithData;

internal sealed class ChatWithDataUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}
