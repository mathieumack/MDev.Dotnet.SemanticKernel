using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Phi3.Diagnostics;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Phi3;

public static class AIStudioMaaSPhi3ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Azure OpenAI chat completion service to the list.
    /// </summary>
    /// <param name="builder">The <see cref="IKernelBuilder"/> instance to augment.</param>
    /// <param name="deploymentName">Azure OpenAI deployment name, see https://learn.microsoft.com/azure/cognitive-services/openai/how-to/create-resource</param>
    /// <param name="endpoint">Azure OpenAI deployment URL, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="apiKey">Azure OpenAI API key, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="serviceId">A local identifier for the given AI service</param>
    /// <param name="modelId">Model identifier, see https://learn.microsoft.com/azure/cognitive-services/openai/quickstart</param>
    /// <param name="httpClient">The HttpClient to use with this service.</param>
    /// <returns>The same instance as <paramref name="builder"/>.</returns>
    public static IKernelBuilder AddAzureAIStudioPhi3ChatCompletion(
        this IKernelBuilder builder,
        string endpoint,
        string apiKey,
        string? serviceId = null,
        string? modelId = null)
    {
        Verify.NotNull(builder);
        Verify.NotNullOrWhiteSpace(endpoint);
        Verify.NotNullOrWhiteSpace(apiKey);

        var deploymentIdenfifier = Guid.NewGuid().ToString();

        var handler = new HttpClientHandler()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) => { return true; }
        };
        builder.Services.AddHttpClient($"http-{modelId}-{deploymentIdenfifier}")
            .ConfigureHttpClient((services, client) =>
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri(endpoint);
            })
            .ConfigurePrimaryHttpMessageHandler(() => handler);

        Func<IServiceProvider, object?, AzureAIStudioMaaSPhi3ChatCompletionService> factory = (serviceProvider, _) =>
        {
            var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient($"http-{modelId}-{deploymentIdenfifier}");

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory?.CreateLogger(typeof(AzureAIStudioMaaSPhi3ChatCompletionService));

            var client = new AzureAIStudioMaaSPhi3ChatCompletionService(httpClient, logger);
            return client;
        };

        builder.Services.AddKeyedSingleton<IChatCompletionService>(serviceId, factory);

        return builder;
    }
}
