using JSLTSharp;
using JSLTSharp.JsonTransforms.Extensions;
using JSLTSharp.JsonTransforms.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Gpto1.JSLTSharpOperations;
using MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Gpto1.HttpClientHandlers;

namespace MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Gpto1.ServiceCollectionExtensions;

public static class AIStudioMaaSGPTo1Extensions
{
    /// <summary>
    /// Register the custom http client to interact with Azure OpenAI o1 models
    /// </summary>
    /// <param name="services"></param>
    /// <param name="httpClientName">The name of the custom http client</param>
    /// <returns></returns>
    public static IServiceCollection RegisterAIStudioMaasGPTo1(this IServiceCollection services, string httpClientName)
    {
        services.AddSingleton<IJsonTransformCustomOperation, RoleRenameOperation>();
        services.AddSingleton<AzureOpenAIHttpClientHandler>();
        services.AddSingleton<JsonTransform>();
        
        services.AddHttpClient(httpClientName)
            .ConfigureHttpClient((provider, client) =>
            {
            })
       .AddHttpMessageHandler<AzureOpenAIHttpClientHandler>();

        return services;
    }
}
