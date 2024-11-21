using System.Text.Json.Serialization;

namespace MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Gpt4o1.ChatCompletionWithData
{
    internal sealed class ChatWithDataChoice
    {
        [JsonPropertyName("message")]
        public ChatWithDataMessage Message { get; set; }
    }
}
