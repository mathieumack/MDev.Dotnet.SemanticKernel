using JSLTSharp.JsonTransforms.Abstractions;
using Newtonsoft.Json.Linq;

namespace MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Gpto1.JSLTSharpOperations;

internal class RoleRenameOperation : IJsonTransformCustomOperation
{
    /// <inheritdoc />
    public virtual string OperationName => "RenameRole";

    /// <inheritdoc />
    public virtual JToken Apply(JToken dataSource, JToken objectToApplyTo, IList<string> parameters)
    {
        if (objectToApplyTo.Type != JTokenType.String)
            return objectToApplyTo;

        var value = objectToApplyTo.ToString();
        return value == "system" ? "user" : value;
    }
}