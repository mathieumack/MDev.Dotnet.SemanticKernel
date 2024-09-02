using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Phi3.ChatCompletionWithData;
using Azure;
using Azure.AI.OpenAI;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging.Abstractions;
using Azure.Core;
using Azure.Core.Pipeline;
using MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Phi3.AzureCore;
using System.Net.Http;
using MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Phi3.ChatCompletion;

namespace MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Phi3;

internal class AzureAIStudioMaaSPhi3ChatCompletionService : IChatCompletionService
{
    private static readonly string[] AuthorizationScopes = new string[1] { "https://cognitiveservices.azure.com/.default" };

    /// <summary>
    /// Logger instance
    /// </summary>
    internal ILogger Logger { get; set; }

    private readonly HttpPipeline _pipeline;
    private readonly Uri endpoint;

    private static RequestContext DefaultRequestContext = new RequestContext();
    private static ResponseClassifier _responseClassifier200;

    private static ResponseClassifier ResponseClassifier200
    {
        get
        {
            ResponseClassifier responseClassifier = _responseClassifier200;
            if (responseClassifier == null)
            {
                responseClassifier = (_responseClassifier200 = new StatusCodeClassifier(stackalloc ushort[1] { 200 }));
            }

            return responseClassifier;
        }
    }

    internal AzureAIStudioMaaSPhi3ChatCompletionService(Uri endpoint, 
                                                        AzureKeyCredential keyCredential,
                                                        Phi3ClientOptions options,
                                                        HttpClient httpClient = null,
                                                        ILogger? logger = null)
    {
        this.endpoint = endpoint;
        if (options == null)
        {
            options = new Phi3ClientOptions();
        }

        if (httpClient is not null)
        {
            options.Transport = new HttpClientTransport(httpClient);
            options.RetryPolicy = new RetryPolicy(maxRetries: 0); // Disable Azure SDK retry policy if and only if a custom HttpClient is provided.
            options.Retry.NetworkTimeout = Timeout.InfiniteTimeSpan; // Disable Azure SDK default timeout
        }

        _pipeline = HttpPipelineBuilder.Build(options, Array.Empty<HttpPipelinePolicy>(), new HttpPipelinePolicy[1]
        {
            new MDEVAzureKeyCredentialPolicy(keyCredential, "api-key")
        }, new ResponseClassifier());
        this.Logger = logger ?? NullLogger.Instance;
    }

    internal AzureAIStudioMaaSPhi3ChatCompletionService(Uri endpoint, 
                                                        TokenCredential tokenCredential,
                                                        Phi3ClientOptions options,
                                                        HttpClient httpClient = null,
                                                        ILogger? logger = null)
    {
        this.endpoint = endpoint;
        if (options == null)
        {
            options = new Phi3ClientOptions();
        }

        if (httpClient is not null)
        {
            options.Transport = new HttpClientTransport(httpClient);
            options.RetryPolicy = new RetryPolicy(maxRetries: 0); // Disable Azure SDK retry policy if and only if a custom HttpClient is provided.
            options.Retry.NetworkTimeout = Timeout.InfiniteTimeSpan; // Disable Azure SDK default timeout
        }

        _pipeline = HttpPipelineBuilder.Build(options, Array.Empty<HttpPipelinePolicy>(), new HttpPipelinePolicy[1]
        {
            new BearerTokenAuthenticationPolicy(tokenCredential, AuthorizationScopes)
        }, new ResponseClassifier());
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

    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        if (chatHistory is null)
            throw new ArgumentNullException(nameof(chatHistory));

        // Convert the incoming execution settings to OpenAI settings.
        var chatExecutionSettings = Phi3PromptExecutionSettings.FromExecutionSettings(executionSettings);
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

        var requestContent = RequestContent.Create(body);

        RequestContext requestContext = FromCancellationToken(cancellationToken);
        try
        {
            using HttpMessage message = CreatePostRequestMessage("chat/completions", requestContent, requestContext);
            var response = ProcessMessage(_pipeline, message, requestContext);

            using JsonDocument jsonDocument = JsonDocument.Parse(response.Content);
            var responseContent = System.Text.Json.JsonSerializer.Deserialize<ChatWithDataResponse>(jsonDocument);
            this.CaptureUsageDetails(responseContent.Usage);
            return responseContent.Choices.Select(e =>
                    ToChatMessageContent(responseContent, e)).ToList();
        }
        catch (Exception exception)
        {
            throw;
        }
    }

    public Response ProcessMessage(HttpPipeline pipeline, HttpMessage message, RequestContext? requestContext, CancellationToken cancellationToken = default(CancellationToken))
    {
        var (cancellationToken2, errorOptions) = ApplyRequestContext(requestContext);
        if (!cancellationToken2.CanBeCanceled || !cancellationToken.CanBeCanceled)
        {
            pipeline.Send(message, cancellationToken.CanBeCanceled ? cancellationToken : cancellationToken2);
        }
        else
        {
            using CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken2, cancellationToken);
            pipeline.Send(message, cancellationTokenSource.Token);
        }

        if (!message.Response.IsError || errorOptions == ErrorOptions.NoThrow)
        {
            return message.Response;
        }

        throw new RequestFailedException(message.Response);
    }

    private (CancellationToken CancellationToken, ErrorOptions ErrorOptions) ApplyRequestContext(RequestContext? requestContext)
    {
        if (requestContext == null)
        {
            return (CancellationToken.None, ErrorOptions.Default);
        }

        return (requestContext.CancellationToken, requestContext.ErrorOptions);
    }

    internal RequestContext FromCancellationToken(CancellationToken cancellationToken = default(CancellationToken))
    {
        if (!cancellationToken.CanBeCanceled)
        {
            return DefaultRequestContext;
        }

        return new RequestContext
        {
            CancellationToken = cancellationToken
        };
    }

    internal RequestUriBuilder GetUri(string operationPath)
    {
        var builder = new RequestUriBuilder();
        builder.Reset(endpoint);
        builder.AppendPath("/" + operationPath, escape: false);
        return builder;
    }

    internal HttpMessage CreatePostRequestMessage(string operationPath, RequestContent content, RequestContext context)
    {
        HttpMessage httpMessage = _pipeline.CreateMessage(context, ResponseClassifier200);
        Request request = httpMessage.Request;
        request.Method = RequestMethod.Post;
        request.Uri = GetUri(operationPath);
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Content-Type", "application/json");
        request.Content = content;
        return httpMessage;
    }
}