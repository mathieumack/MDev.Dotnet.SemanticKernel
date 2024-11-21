using System.Text.Json.Serialization;

namespace MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Phi3.ChatCompletionWithData
{
    internal sealed class ChatWithDataChoice
    {
        [JsonPropertyName("message")]
        public ChatWithDataMessage Message { get; set; }
    }
}
