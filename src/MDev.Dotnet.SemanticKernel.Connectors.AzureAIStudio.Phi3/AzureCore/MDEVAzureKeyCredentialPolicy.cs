using Azure.Core.Pipeline;
using Azure.Core;
using Azure;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;

namespace MDev.Dotnet.SemanticKernel.Connectors.AzureAIStudio.Phi3.AzureCore
{
    internal class MDEVAzureKeyCredentialPolicy : HttpPipelineSynchronousPolicy
    {
        private readonly string _name;
        private readonly AzureKeyCredential _credential;
        private readonly string? _prefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="MDEVAzureKeyCredentialPolicy"/> class.
        /// </summary>
        /// <param name="credential">The <see cref="AzureKeyCredential"/> used to authenticate requests.</param>
        /// <param name="name">The name of the key header used for the credential.</param>
        /// <param name="prefix">The prefix to apply before the credential key. For example, a prefix of "SharedAccessKey" would result in
        /// a value of "SharedAccessKey {credential.Key}" being stamped on the request header with header key of <paramref name="name"/>.</param>
        public MDEVAzureKeyCredentialPolicy(AzureKeyCredential credential, string name, string? prefix = null)
        {
            if (credential is null)
            {
                throw new ArgumentNullException(nameof(credential));
            }
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (name.Length == 0)
            {
                throw new ArgumentException("Value cannot be an empty string.", nameof(name));
            }
            _credential = credential;
            _name = name;
            _prefix = prefix;
        }

        /// <inheritdoc/>
        public override void OnSendingRequest(HttpMessage message)
        {
            base.OnSendingRequest(message);
            message.Request.Headers.SetValue(_name, _prefix != null ? $"{_prefix} {_credential.Key}" : _credential.Key);
        }
    }
}
