using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Gpt4o1.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Microsoft.Extensions.Http.Logging;
using System.Diagnostics.CodeAnalysis;

namespace MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Gpt4o1;

public static class AIStudioMaaSGpt4o1ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Azure OpenAI chat completion service to the list.
    /// </summary>
    /// <param name="builder">The <see cref="IKernelBuilder"/> instance to augment.</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="apiKey">Azure OpenAI API key, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <returns>The same instance as <paramref name="builder"/>.</returns>
    [Experimental("SKMDEVEXP0002")]
    public static IKernelBuilder AddAzureAIStudioGpt4o1ChatCompletion(
        this IKernelBuilder builder,
        string endpoint,
        string apiKey,
        string? serviceId = null)
    {
        Verify.NotNull(builder);
        Verify.NotNullOrWhiteSpace(endpoint);
        Verify.NotNullOrWhiteSpace(apiKey);

        var deploymentIdenfifier = Guid.NewGuid().ToString();

        builder.Services.AddHttpClient($"http--{deploymentIdenfifier}");

        Func<IServiceProvider, object?, AzureAIStudioMaaSGpt4o1ChatCompletionService> factory = (serviceProvider, _) =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient($"http-{deploymentIdenfifier}");

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger(typeof(AzureAIStudioMaaSGpt4o1ChatCompletionService));

            var client = new AzureAIStudioMaaSGpt4o1ChatCompletionService(new Uri(endpoint), 
                                                                        new Azure.AzureKeyCredential(apiKey),
                                                                        new(),
                                                                        httpClient,
                                                                        logger);
            return client;
        };

        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, factory);

        return builder;
    }

    /// <summary>
    /// Adds the Azure OpenAI chat completion service to the list.
    /// </summary>
    /// <param name="builder">The <see cref="IKernelBuilder"/> instance to augment.</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="apiKey">Azure OpenAI API key, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="httpClient">The HttpClient to use with this service.</param>
    /// <returns>The same instance as <paramref name="builder"/>.</returns>
    [Experimental("SKMDEVEXP0002")]
    public static IKernelBuilder AddAzureAIStudioGpt4o1ChatCompletion(
        this IKernelBuilder builder,
        string endpoint,
        string apiKey,
        HttpClient httpClient = null,
        string? serviceId = null)
    {
        Verify.NotNull(builder);
        Verify.NotNullOrWhiteSpace(endpoint);
        Verify.NotNullOrWhiteSpace(apiKey);

        Func<IServiceProvider, object?, AzureAIStudioMaaSGpt4o1ChatCompletionService> factory = (serviceProvider, _) =>
        {
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger(typeof(AzureAIStudioMaaSGpt4o1ChatCompletionService));

            var client = new AzureAIStudioMaaSGpt4o1ChatCompletionService(new Uri(endpoint),
                                                                        new Azure.AzureKeyCredential(apiKey),
                                                                        new(),
                                                                        httpClient,
                                                                        logger);
            return client;
        };

        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, factory);

        return builder;
    }

    /// <summary>
    /// Adds the Azure OpenAI chat completion service to the list.
    /// </summary>
    /// <param name="builder">The <see cref="IKernelBuilder"/> instance to augment.</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="credentials">Azure credentials that can be used for a managed identity or for current user</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="httpClient">The HttpClient to use with this service.</param>
    /// <returns>The same instance as <paramref name="builder"/>.</returns>
    [Experimental("SKMDEVEXP0002")]
    public static IKernelBuilder AddAzureAIStudioGpt4o1ChatCompletion(
        this IKernelBuilder builder,
        string endpoint,
        DefaultAzureCredential credentials,
        HttpClient httpClient = null,
        string? serviceId = null)
    {
        Verify.NotNull(builder);
        Verify.NotNullOrWhiteSpace(endpoint);
        Verify.NotNull(credentials);

        Func<IServiceProvider, object?, AzureAIStudioMaaSGpt4o1ChatCompletionService> factory = (serviceProvider, _) =>
        {
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger(typeof(AzureAIStudioMaaSGpt4o1ChatCompletionService));

            var client = new AzureAIStudioMaaSGpt4o1ChatCompletionService(new Uri(endpoint),
                                                                        credentials,
                                                                        new(),
                                                                        httpClient, 
                                                                        logger);
            return client;
        };

        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, factory);

        return builder;
    }
}
