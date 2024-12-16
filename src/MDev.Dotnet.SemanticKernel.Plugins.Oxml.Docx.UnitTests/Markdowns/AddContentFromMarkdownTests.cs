using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using MarkdownToDocxGenerator.Extensions;
using MDev.Dotnet.SemanticKernel.Plugins.Oxml.Docx.ServiceFunctions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace MDev.Dotnet.SemanticKernel.Plugins.Oxml.Docx.UnitTests.Markdowns
{
    [TestClass]
    public sealed class AddContentFromMarkdownTests
    {
        /// <summary>
        /// Test to create valid documents
        /// </summary>
        /// <param name="documentName"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        [TestMethod("Create valid documents")]
        [DataRow("addcontentfrommarkdown_fr.docx", "fr-FR", DisplayName = "Create document with markdown")]
        public async Task AddContentFromMarkdown_Valid(string documentName, string culture)
        {
            // Arrange
            var provider = await UnitTestInitializer.CreateProviderAsync((sp) =>
            {
                sp.AddScoped<WordPluginServiceFunctions>();
                sp.RegisterMarkdownToDocxGenerator(true);
            });
            var service = provider.GetRequiredService<WordPluginServiceFunctions>();
            var kernel = provider.GetRequiredService<Kernel>();
            var reference = await service.CreateDocument(kernel, documentName, culture);
            // Create a sample of markdown :
            var sampleMarkdown = GetSampleMarkdown();

            // Act
            await service.AddContentFromMarkdown(kernel, reference, sampleMarkdown);
            var base64 = await service.SaveContent(kernel, reference);

            // Asserta
            Assert.IsNotNull(base64);
            // Save base64 file locally :
            var path = documentName + "_" + Guid.NewGuid().ToString() + ".docx";
            File.WriteAllBytes(path, Convert.FromBase64String(base64));
            using (var wordDoc = WordprocessingDocument.Open(path, false))
            {
                var validator = new OpenXmlValidator();
                var errors = validator.Validate(wordDoc);
            }
        }

        /// <summary>
        /// Generate a sample of markdown content to insert
        /// </summary>
        /// <returns></returns>
        private string GetSampleMarkdown()
        {
            return 
""""
# Sample title for a random markdown file

## Overview

Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed euismod, nisl quis tincidunt aliquam, nunc nisl ultricies nunc, quis aliquam nis

## Function Signature

Here is a random code in python that generate a random value. It takes some parameters like a date and return a value.

```python
def random_function(date: datetime) -> float:
    """Generate a random value based on the provided date.

    Args:
        date (datetime): The date to be used to generate the random value.

    Returns:
        float: The random value generated based on the provided date.
    """
    return random.random()
```

## Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| date | datetime | The date to be used to generate the random value. |

## Return Value

| Type | Description |
| ---- | ----------- |
| float | The random value generated based on the provided date. |

## Usage

```python
random_function(datetime.now())
```

## Example Output

```python
0.123456789
```
"""";
        }
    }
}
