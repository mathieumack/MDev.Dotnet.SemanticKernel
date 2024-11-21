using Microsoft.SemanticKernel;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Gpt4o1.ChatCompletionWithData;

[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public sealed class Gpt4o1PromptExecutionSettings : PromptExecutionSettings
{
    /// <summary>
    /// Temperature controls the randomness of the completion.
    /// The higher the temperature, the more random the completion.
    /// Default is 1.0.
    /// </summary>
    [JsonPropertyName("temperature")]
    public double Temperature
    {
        get => this._temperature;

        set
        {
            this.ThrowIfFrozen();
            this._temperature = value;
        }
    }

    /// <summary>
    /// TopP controls the diversity of the completion.
    /// The higher the TopP, the more diverse the completion.
    /// Default is 1.0.
    /// </summary>
    [JsonPropertyName("top_p")]
    public double TopP
    {
        get => this._topP;

        set
        {
            this.ThrowIfFrozen();
            this._topP = value;
        }
    }

    /// <summary>
    /// The maximum number of tokens to generate in the completion.
    /// </summary>
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens
    {
        get => this._maxTokens;

        set
        {
            this.ThrowIfFrozen();
            this._maxTokens = value;
        }
    }

    ///// <summary>
    ///// A unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse
    ///// </summary>
    //public string? User
    //{
    //    get => this._user;

    //    set
    //    {
    //        this.ThrowIfFrozen();
    //        this._user = value;
    //    }
    //}

    /// <inheritdoc/>
    public override void Freeze()
    {
        if (this.IsFrozen)
        {
            return;
        }

        base.Freeze();
    }

    /// <inheritdoc/>
    public override PromptExecutionSettings Clone()
    {
        return new OpenAIPromptExecutionSettings()
        {
            ModelId = this.ModelId,
            ExtensionData = this.ExtensionData is not null ? new Dictionary<string, object>(this.ExtensionData) : null,
            Temperature = this.Temperature,
            TopP = this.TopP,
            MaxTokens = this.MaxTokens,
            //User = this.User
        };
    }

    /// <summary>
    /// Default max tokens for a text generation
    /// </summary>
    internal static int DefaultTextMaxTokens { get; } = 256;

    /// <summary>
    /// Create a new settings object with the values from another settings object.
    /// </summary>
    /// <param name="executionSettings">Template configuration</param>
    /// <param name="defaultMaxTokens">Default max tokens</param>
    /// <returns>An instance of OpenAIPromptExecutionSettings</returns>
    public static OpenAIPromptExecutionSettings FromExecutionSettings(PromptExecutionSettings? executionSettings, int? defaultMaxTokens = null)
    {
        if (executionSettings is null)
        {
            return new OpenAIPromptExecutionSettings()
            {
                MaxTokens = defaultMaxTokens
            };
        }

        if (executionSettings is OpenAIPromptExecutionSettings settings)
        {
            return settings;
        }

        var json = JsonSerializer.Serialize(executionSettings);

        //var jsonOptions = new JsonSerializerOptions()
        //{
            
        //};
        var openAIExecutionSettings = JsonSerializer.Deserialize<OpenAIPromptExecutionSettings>(json);
        if (openAIExecutionSettings is not null)
        {
            return openAIExecutionSettings;
        }

        throw new ArgumentException($"Invalid execution settings, cannot convert to {nameof(OpenAIPromptExecutionSettings)}", nameof(executionSettings));
    }

    #region private ================================================================================

    private double _temperature = 1;
    private double _topP = 1;
    private int? _maxTokens;
    //private string? _user;

    #endregion
}
