using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Phi3.ChatCompletionWithData;
using Azure;
using MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Phi3.Exceptions;
using Azure.AI.OpenAI;
using System.Diagnostics.Metrics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Logging.Abstractions;

namespace MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Phi3;

internal class AzureAIStudioMaaSPhi3ChatCompletionService : IChatCompletionService
{
    /// <summary>
    /// Logger instance
    /// </summary>
    internal ILogger Logger { get; set; }

    private readonly HttpClient httpClient;

    internal AzureAIStudioMaaSPhi3ChatCompletionService(HttpClient httpClient, ILogger? logger = null)
    {
        this.httpClient = httpClient;
        this.Logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Instance of <see cref="Meter"/> for metrics.
    /// </summary>
    private static readonly Meter s_meter = new("Microsoft.SemanticKernel.Connectors.OpenAI");

    /// <summary>
    /// Instance of <see cref="Counter{T}"/> to keep track of the number of prompt tokens used.
    /// </summary>
    private static readonly Counter<int> s_promptTokensCounter =
        s_meter.CreateCounter<int>(
            name: "semantic_kernel.connectors.aistudio.phi3.tokens.prompt",
            unit: "{token}",
            description: "Number of prompt tokens used");

    /// <summary>
    /// Instance of <see cref="Counter{T}"/> to keep track of the number of completion tokens used.
    /// </summary>
    private static readonly Counter<int> s_completionTokensCounter =
        s_meter.CreateCounter<int>(
            name: "semantic_kernel.connectors.aistudio.phi3.tokens.completion",
            unit: "{token}",
            description: "Number of completion tokens used");

    /// <summary>
    /// Instance of <see cref="Counter{T}"/> to keep track of the total number of tokens used.
    /// </summary>
    private static readonly Counter<int> s_totalTokensCounter =
        s_meter.CreateCounter<int>(
            name: "semantic_kernel.connectors.aistudio.phi3.tokens.total",
            unit: "{token}",
            description: "Number of tokens used");

    public IReadOnlyDictionary<string, object?> Attributes => throw new NotImplementedException();

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        if (chatHistory is null)
            throw new ArgumentNullException(nameof(chatHistory));

        // Convert the incoming execution settings to OpenAI settings.
        var chatExecutionSettings = Phi3PromptExecutionSettings.FromExecutionSettings(executionSettings);
        bool autoInvoke = false;
        ValidateMaxTokens(chatExecutionSettings.MaxTokens);

        // Create the Azure SDK ChatCompletionOptions instance from all available information.
        var body = new ChatWithDataRequest()
        {
            MaxTokens = chatExecutionSettings.MaxTokens,
            Temperature = chatExecutionSettings.Temperature,
            TopP = chatExecutionSettings.TopP,
            Messages = chatHistory.Select(e => new ChatWithDataMessage()
            {
                Role = e.Role.ToString(),
                Content = e.Content.ToString()
            }).ToList()
        };

        var uri = "v1/chat/completions";

        var requestBody = JsonSerializer.Serialize(body);

        var content = new StringContent(requestBody);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        for (int iteration = 1; ; iteration++)
        {
            // Make the request.
            HttpResponseMessage response = await httpClient.PostAsync(uri, content);

            ChatWithDataResponse responseContent = null;

            if (response.IsSuccessStatusCode)
            {
                responseContent = await response.Content.ReadFromJsonAsync<ChatWithDataResponse>();
            }
            //else
            //{
            //    //Console.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));

            //    // Print the headers - they include the requert ID and the timestamp,
            //    // which are useful for debugging the failure
            //    //Console.WriteLine(response.Headers.ToString());

            //    responseContent = await response.Content.ReadAsStringAsync();
            //    //Console.WriteLine(responseContent);
            //}
            this.CaptureUsageDetails(responseContent.Usage);

            if (responseContent is null)
            {
                throw new KernelException("Chat completions not found");
            }

            // If we don't want to attempt to invoke any functions, just return the result.
            // Or if we are auto-invoking but we somehow end up with other than 1 choice even though only 1 was requested, similarly bail.
            return responseContent.Choices.Select(e =>
                    ToChatMessageContent(responseContent, e)).ToList();
        }
    }

    private ChatMessageContent ToChatMessageContent(ChatWithDataResponse chatWithDataResponse, ChatWithDataChoice chatWithDataChoice)
    {
        var metadatas = new Dictionary<string, object>();

        // Add Usage :
        metadatas.Add("Usage", new Phi3CompletionUsage(chatWithDataResponse.Usage.CompletionTokens,
                                                        chatWithDataResponse.Usage.PromptTokens,
                                                        chatWithDataResponse.Usage.TotalTokens));

        var result = new ChatMessageContent()
        {
            Content = chatWithDataChoice.Message.Content,
            Role = AuthorRole.Assistant,
            ModelId = chatWithDataResponse.Model,
            Metadata = metadatas
        };

        return result;
    }

    private static void ValidateMaxTokens(int? maxTokens)
    {
        if (maxTokens.HasValue && maxTokens < 1)
        {
            throw new ArgumentException($"MaxTokens {maxTokens} is not valid, the value must be greater than zero");
        }
    }

    /// <summary>
    /// Captures usage details, including token information.
    /// </summary>
    /// <param name="usage">Instance of <see cref="CompletionsUsage"/> with usage details.</param>
    private void CaptureUsageDetails(ChatWithDataUsage usage)
    {
        if (usage is null)
        {
            if (this.Logger.IsEnabled(LogLevel.Debug))
            {
                this.Logger.LogDebug("Usage information is not available.");
            }

            return;
        }

        if (this.Logger.IsEnabled(LogLevel.Information))
        {
            this.Logger.LogInformation(
                "Prompt tokens: {PromptTokens}. Completion tokens: {CompletionTokens}. Total tokens: {TotalTokens}.",
                usage.PromptTokens, usage.CompletionTokens, usage.TotalTokens);
        }

        s_promptTokensCounter.Add(usage.PromptTokens);
        s_completionTokensCounter.Add(usage.CompletionTokens);
        s_totalTokensCounter.Add(usage.TotalTokens);
    }

    //private static async Task<T> RunRequestAsync<T>(Func<Task<T>> request)
    //{
    //    try
    //    {
    //        return await request.Invoke().ConfigureAwait(false);
    //    }
    //    catch (RequestFailedException e)
    //    {
    //        throw e.ToHttpOperationException();
    //    }
    //}

    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
