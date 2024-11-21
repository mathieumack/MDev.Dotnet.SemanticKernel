using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Gpt4o1.ChatCompletionWithData;

public class Gpt4o1CompletionUsage
{
    /// <summary>
    /// The number of tokens generated across all completions emissions.
    /// </summary>
    public int CompletionTokens { get; }

    /// <summary>
    /// The number of tokens in the provided prompts for the completions request.
    /// </summary>
    public int PromptTokens { get; }

    /// <summary>
    /// The total number of tokens processed for the completions request and response.
    /// </summary>
    public int TotalTokens { get; }

    /// <summary>
    /// Initializes a new instance of Azure.AI.OpenAI.CompletionsUsage.
    /// </summary>
    /// <param name="completionTokens">The number of tokens generated across all completions emissions.</param>
    /// <param name="promptTokens">The number of tokens in the provided prompts for the completions request.</param>
    /// <param name="totalTokens">The total number of tokens processed for the completions request and response.</param>
    internal Gpt4o1CompletionUsage(int completionTokens, int promptTokens, int totalTokens)
    {
        CompletionTokens = completionTokens;
        PromptTokens = promptTokens;
        TotalTokens = totalTokens;
    }
}
