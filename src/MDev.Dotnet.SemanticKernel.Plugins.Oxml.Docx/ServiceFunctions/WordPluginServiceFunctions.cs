using System.ComponentModel;
using OpenXMLSDK.Engine.Word;
using Microsoft.Extensions.Logging;
using MDev.Dotnet.SemanticKernel.Plugins.Oxml.Docx.Oxml.DTOs;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using MarkdownToDocxGenerator;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenXMLSDK.Engine.Word.Bookmarks;

namespace MDev.Dotnet.SemanticKernel.Plugins.Oxml.Docx.ServiceFunctions;

public class WordPluginServiceFunctions
{
    private readonly ILogger<WordPluginServiceFunctions> logger;

    public WordPluginServiceFunctions(
                    ILogger<WordPluginServiceFunctions> logger)
    {
        this.logger = logger;
    }

    public const string CreateFunctionName = "create";
    [KernelFunction(CreateFunctionName)]
    [Description("Creates a new Word document. Use other functions to add content to it.")]
    [return: Description("A reference to the created document, which can be used with other functions.")]
    public Task<string> CreateDocument(Kernel kernel,
                                        [Description("The name of the document.")] string documentName,
                                        [Description("The default culture for content generation, e.g., en-US.")] string defaultCulture = "en-US")
    {
        if (string.IsNullOrWhiteSpace(documentName))
        {
            logger.LogError("Document name is required");
            return Task.FromResult("Document name is required");
        }

        // Create a new WordManager document :
        var wordDocumentReference = new SKWordDocument()
        {
            Id = Guid.NewGuid().ToString(),
            DocumentName = documentName,
            Culture = new System.Globalization.CultureInfo(defaultCulture ?? "en-US"),
            Document = new WordManager()
        };
        wordDocumentReference.Document.New();

        kernel.Data.Add(wordDocumentReference.Id, wordDocumentReference);

        return Task.FromResult(wordDocumentReference.Id);
    }

    public const string AppendMarkdownFunctionName = "appendmarkdown";
    [KernelFunction(AppendMarkdownFunctionName)]
    [Description("Appends markdown content to an existing Word document.")]
    [return: Description("A reference to the document, which can be used with other functions.")]
    public Task<string> AddContentFromMarkdown(Kernel kernel,
                                    [Description("The reference to the document.")] string reference,
                                    [Description("The content in markdown format.")] string content)
    {
        var document = GetDocument(kernel, reference);
        if (document is null)
            return Task.FromResult("Document reference not found");

        var parser = kernel.Services.GetRequiredService<MdToOxmlEngine>();

        var report = parser.Transform(content, "");
        document.Document.AppendSubDocument(new List<OpenXMLSDK.Engine.Word.ReportEngine.Report>() { report }, true, document.Culture);

        return Task.FromResult(reference);
    }

    public const string SetTextOnBookmarkFunctionName = "settexonbokmark";
    [KernelFunction(SetTextOnBookmarkFunctionName)]
    [Description("")]
    [return: Description("A reference to the document, which can be used with other functions.")]
    public Task<string> SetTextOnBookmark(Kernel kernel,
                                    [Description("The reference to the document.")] string reference,
                                    [Description("The bookmark name.")] string bookmarkName,
                                    [Description("The text content to insert.")] string content)
    {
        var document = GetDocument(kernel, reference);
        if (document is null)
            return Task.FromResult("Document reference not found");

        document.Document.SetTextOnBookmark(bookmarkName, content);

        return Task.FromResult(reference);
    }

    public const string SaveFunctionName = "save";
    [KernelFunction(SaveFunctionName)]
    [Description("Saves the document and returns it as a Base64 string.")]
    [return: Description("A Base64 string representing the document.")]
    public Task<string> SaveContent(Kernel kernel,
                                    [Description("The reference to the document.")] string reference)
    {
        var document = GetDocument(kernel, reference);
        if (document is null)
            return Task.FromResult("Document reference not found");

        // Close doc, and get stream for saving:
        document.Document.SaveDoc();
        var documentStream = document.Document.GetMemoryStream();

        var result = Convert.ToBase64String(documentStream.ToArray());

        return Task.FromResult(result);
    }

    /// <summary>
    /// Retrieves the document from the kernel data using the provided reference.
    /// </summary>
    /// <param name="kernel">The kernel instance.</param>
    /// <param name="reference">The reference to the document.</param>
    /// <returns>The document if found; otherwise, null.</returns>
    private SKWordDocument GetDocument(Kernel kernel, string reference)
    {
        if (!kernel.Data.ContainsKey(reference) || !(kernel.Data[reference] is SKWordDocument))
        {
            logger.LogWarning("Document reference not found");
            return null;
        }
        return kernel.Data[reference] as SKWordDocument;
    }
}