using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Phi3.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Azure.Identity;

namespace MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Phi3;

public static class AIStudioMaaSPhi3ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Azure OpenAI chat completion service to the list.
    /// </summary>
    /// <param name="builder">The <see cref="IKernelBuilder"/> instance to augment.</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="apiKey">Azure OpenAI API key, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <returns>The same instance as <paramref name="builder"/>.</returns>
    public static IKernelBuilder AddAzureAIStudioPhi3ChatCompletion(
        this IKernelBuilder builder,
        string endpoint,
        string apiKey,
        string? serviceId = null)
    {
        Verify.NotNull(builder);
        Verify.NotNullOrWhiteSpace(endpoint);
        Verify.NotNullOrWhiteSpace(apiKey);

        var deploymentIdenfifier = Guid.NewGuid().ToString();

        builder.Services.AddHttpClient($"http--{deploymentIdenfifier}")
            .ConfigureHttpClient((services, client) =>
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri(endpoint);
            });

        Func<IServiceProvider, object?, AzureAIStudioMaaSPhi3ChatCompletionService> factory = (serviceProvider, _) =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient($"http-{deploymentIdenfifier}");

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger(typeof(AzureAIStudioMaaSPhi3ChatCompletionService));

            var client = new AzureAIStudioMaaSPhi3ChatCompletionService(httpClient, null, logger);
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
    public static IKernelBuilder AddAzureAIStudioPhi3ChatCompletion(
        this IKernelBuilder builder,
        string endpoint,
        string apiKey,
        HttpClient httpClient,
        string? serviceId = null)
    {
        Verify.NotNull(builder);
        Verify.NotNullOrWhiteSpace(endpoint);
        Verify.NotNullOrWhiteSpace(apiKey);

        httpClient.BaseAddress = new Uri(endpoint);

        Func<IServiceProvider, object?, AzureAIStudioMaaSPhi3ChatCompletionService> factory = (serviceProvider, _) =>
        {
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger(typeof(AzureAIStudioMaaSPhi3ChatCompletionService));

            var client = new AzureAIStudioMaaSPhi3ChatCompletionService(httpClient, null, logger);
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
    public static IKernelBuilder AddAzureAIStudioPhi3ChatCompletion(
        this IKernelBuilder builder,
        string endpoint,
        DefaultAzureCredential credentials,
        HttpClient httpClient,
        string? serviceId = null)
    {
        Verify.NotNull(builder);
        Verify.NotNullOrWhiteSpace(endpoint);
        Verify.NotNull(credentials);

        httpClient.BaseAddress = new Uri(endpoint);

        Func<IServiceProvider, object?, AzureAIStudioMaaSPhi3ChatCompletionService> factory = (serviceProvider, _) =>
        {
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger(typeof(AzureAIStudioMaaSPhi3ChatCompletionService));

            var client = new AzureAIStudioMaaSPhi3ChatCompletionService(httpClient, credentials, logger);
            return client;
        };

        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, factory);

        return builder;
    }
}
