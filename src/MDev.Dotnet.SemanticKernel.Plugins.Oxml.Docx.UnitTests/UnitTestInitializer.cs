using Azure.Identity;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using NSubstitute;

namespace MDev.Dotnet.SemanticKernel.Plugins.Oxml.Docx.UnitTests;

internal static class UnitTestInitializer
{
    public static Task<IServiceProvider> CreateProviderAsync(Action<IServiceCollection> registerOtherServices = null)
    {
        var config = new ConfigurationBuilder()
            //.AddJsonFile("appsettings.json")
            //.AddEnvironmentVariables()
            .Build();

        IServiceCollection serviceCollection = new ServiceCollection();

        var options = new DefaultAzureCredentialOptions()
        {
            ExcludeAzureDeveloperCliCredential = true,
            ExcludeAzurePowerShellCredential = true,
            ExcludeEnvironmentCredential = true,
            ExcludeInteractiveBrowserCredential = true,
            ExcludeSharedTokenCacheCredential = true,
            ExcludeVisualStudioCodeCredential = true,
            ExcludeVisualStudioCredential = false,
            ExcludeWorkloadIdentityCredential = true
        };
        var azureCredentials = new DefaultAzureCredential(options);

        // Load configuration :
        serviceCollection.AddLogging()
                            //.BindConfiguration<List<OpenAiSettings>>(config, out var openaiSettings, OpenAiSettings.SectionName)
                            .AddSingleton(TimeProvider.System);

        serviceCollection.AddScoped<Kernel>(sp =>
        {
            var kernelBuilder = GetDefaultKernel(serviceCollection, "");

            var kernel = kernelBuilder.Build();

            return kernel;
        });

        if(registerOtherServices != null)
        {
            registerOtherServices.Invoke(serviceCollection);
        }

        var serviceProvider = serviceCollection.BuildServiceProvider();

        return Task.FromResult<IServiceProvider>(serviceProvider);
    }

    private static IKernelBuilder GetDefaultKernel(this IServiceCollection services, string aiResponse)
    {
        var kernelBuilder = Kernel.CreateBuilder();

        // Copy Application services :
        foreach (var service in services)
        {
            kernelBuilder.Services.Add(service);
        }

        // Arrange 
        var mockChatCompletion = Substitute.For<IChatCompletionService>();
        mockChatCompletion.GetChatMessageContentsAsync(
            Arg.Any<ChatHistory>(),
            Arg.Any<PromptExecutionSettings>(),
            Arg.Any<Kernel>(),
            Arg.Any<CancellationToken>()
        ).Returns([new ChatMessageContent(AuthorRole.Assistant, aiResponse)]);

        kernelBuilder.Services.AddSingleton(mockChatCompletion);

        return kernelBuilder;
    }
}
