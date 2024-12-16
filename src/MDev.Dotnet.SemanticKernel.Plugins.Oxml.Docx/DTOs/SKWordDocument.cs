using OpenXMLSDK.Engine.Word;
using System.Globalization;

namespace MDev.Dotnet.SemanticKernel.Plugins.Oxml.Docx.Oxml.DTOs;

public class SKWordDocument
{
    /// <summary>
    /// Semantic kernel document identifier
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Final document name
    /// </summary>
    public string DocumentName { get; set; }

    /// <summary>
    /// Default generation culture
    /// </summary>
    public CultureInfo Culture { get; set; }

    /// <summary>
    /// Word manager reference
    /// </summary>
    public WordManager Document { get; set; }
}
