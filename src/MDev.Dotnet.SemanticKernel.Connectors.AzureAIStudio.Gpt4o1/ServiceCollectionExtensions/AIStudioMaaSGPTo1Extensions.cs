using JSLTSharp;
using JSLTSharp.JsonTransforms.Extensions;
using JSLTSharp.JsonTransforms.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Gpt4o1.JSLTSharpOperations;
using MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Gpt4o1.HttpClientHandlers;

namespace MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Gpt4o1.ServiceCollectionExtensions;

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
        services.AddScoped<IJsonTransformCustomOperation, RoleRenameOperation>();
        services.AddScoped<AzureOpenAIHttpClientHandler>();
        services.AddScoped<JsonTransform>();
        services.RegisterJsonCustomTransformFunctions();

        services.AddHttpClient(httpClientName)
            .ConfigureHttpClient((provider, client) =>
            {
            })
       .AddHttpMessageHandler<AzureOpenAIHttpClientHandler>();

        return services;
    }
}
