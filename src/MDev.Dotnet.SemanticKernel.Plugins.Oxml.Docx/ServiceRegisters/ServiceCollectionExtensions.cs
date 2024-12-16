using MarkdownToDocxGenerator.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MDev.Dotnet.SemanticKernel.Plugins.Oxml.Docx.ServiceRegisters;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register Oxml module
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection RegisterOxmlModule(this IServiceCollection services,
                                                        IConfiguration configuration)
    {
        services.RegisterMarkdownToDocxGenerator(true);

        return services;
    }
}
