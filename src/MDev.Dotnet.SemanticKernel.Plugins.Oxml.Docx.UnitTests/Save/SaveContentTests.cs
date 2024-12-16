using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using MDev.Dotnet.SemanticKernel.Plugins.Oxml.Docx.ServiceFunctions;
using Microsoft.CodeCoverage.Core.Reports.Coverage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace MDev.Dotnet.SemanticKernel.Plugins.Oxml.Docx.UnitTests.Save;

[TestClass]
public sealed class SaveContentTests
{
    /// <summary>
    /// Test to create valid documents
    /// </summary>
    /// <param name="documentName"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    [TestMethod("Create valid documents")]
    [DataRow("outputdocument_fr.docx", "fr-FR", DisplayName = "Create empty document")]
    public async Task SaveContent_Valid(string documentName, string culture)
    {
        // Arrange
        var provider = await UnitTestInitializer.CreateProviderAsync((sp) =>
        {
            sp.AddScoped<WordPluginServiceFunctions>();
        });
        var service = provider.GetRequiredService<WordPluginServiceFunctions>();
        var kernel = provider.GetRequiredService<Kernel>();
        var reference = await service.CreateDocument(kernel, documentName, culture);

        // Act
        var base64 = await service.SaveContent(kernel, reference);

        // Asserta
        Assert.IsNotNull(base64);
        // Save base64 file locally :
        var path = Guid.NewGuid().ToString() + ".docx";
        File.WriteAllBytes(path, Convert.FromBase64String(base64));
        using (var wordDoc = WordprocessingDocument.Open(path, false))
        {
            var validator = new OpenXmlValidator();
            var errors = validator.Validate(wordDoc);
        }
    }
}
