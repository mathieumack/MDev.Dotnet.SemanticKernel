using MDev.Dotnet.SemanticKernel.Plugins.Oxml.Docx.Oxml.DTOs;
using MDev.Dotnet.SemanticKernel.Plugins.Oxml.Docx.ServiceFunctions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace MDev.Dotnet.SemanticKernel.Plugins.Oxml.Docx.UnitTests.Create;

[TestClass]
public sealed class CreateDocumentTests
{
    /// <summary>
    /// Test to create valid documents
    /// </summary>
    /// <param name="documentName"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    [TestMethod("Create invalid documents")]
    [DataRow("", "fr-FR", DisplayName = "Empty document name")]
    [DataRow(null, "en-US", DisplayName = "Null document name")]
    public async Task CreateDocument_InValid(string documentName, string culture)
    {
        // Arrange
        var provider = await UnitTestInitializer.CreateProviderAsync((sp) =>
        {
            sp.AddScoped<WordPluginServiceFunctions>();
        });
        var service = provider.GetRequiredService<WordPluginServiceFunctions>();
        var kernel = provider.GetRequiredService<Kernel>();

        // Act
        var reference = await service.CreateDocument(kernel, documentName, culture);

        // Assert
        Assert.AreEqual(reference, "Document name is required");
    }

    /// <summary>
    /// Test to create valid documents
    /// </summary>
    /// <param name="documentName"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    [TestMethod("Create valid documents")]
    [DataRow("outputdocument_fr.docx", "fr-FR", DisplayName = "Create empty document")]
    [DataRow("outputdocument_en.docx", "en-US", DisplayName = "Create empty document")]
    [DataRow("outputdocument_en.docx", null, DisplayName = "Create empty document")]
    public async Task CreateDocument_Valid(string documentName, string culture)
    {
        // Arrange
        var provider = await UnitTestInitializer.CreateProviderAsync((sp) =>
        {
            sp.AddScoped<WordPluginServiceFunctions>();
        });
        var service = provider.GetRequiredService<WordPluginServiceFunctions>();
        var kernel = provider.GetRequiredService<Kernel>();

        // Act
        var reference = await service.CreateDocument(kernel, documentName, culture);

        // Assert
        Assert.IsNotNull(reference);

        var document = kernel.Data[reference] as SKWordDocument;
        Assert.IsNotNull(document);
        Assert.AreEqual(documentName, document.DocumentName);
        Assert.AreEqual(culture ?? "en-US", document.Culture.Name);
        Assert.AreEqual(reference, document.Id);
    }
}
