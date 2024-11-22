using JSLTSharp;
using System.Text;

namespace MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Gpt4o1.HttpClientHandlers;

internal class AzureOpenAIHttpClientHandler : DelegatingHandler
{
    private readonly JsonTransform _jsonTransform;

    public AzureOpenAIHttpClientHandler(JsonTransform jsonTransform)
    {
        _jsonTransform = jsonTransform;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var message = await request.Content!.ReadAsStringAsync();
        var output = _jsonTransform.Transform(message,
                    @"{
                       'messages': {
                            '$.messages->loop(entry)': {
                                'role': '$.entry.role->RenameRole()',
                                'content': '$.entry.content'
                            }
                        },
                        'model': '$.model',
                        'temperature': 1, // Forced to 1
                        //'tools': '$.tools', // Not supported
                        //'tool_choice': '$.tool_choice', // Not supported
                        'user': '$.user',
                        //'max_tokens': '$.max_tokens', // Not supported, use max_completion_tokens instead
                        'max_completion_tokens': '$.max_tokens'
                       }
                    ");
        request.Content = new StringContent(output, Encoding.UTF8, "application/json");

        var response = await base.SendAsync(request, cancellationToken);

        return response;
    }
}
