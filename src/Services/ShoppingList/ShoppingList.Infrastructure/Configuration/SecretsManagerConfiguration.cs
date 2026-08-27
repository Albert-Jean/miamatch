using System;
using System.IO;
using System.Text;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;

namespace ShoppingList.Infrastructure.Configuration
{
    public static class SecretsManagerConfiguration
    {
        public const string SecretIdVariable = "MIAMMATCH_SECRET_ID";

        /// <summary>
        /// Adds the AWS Secrets Manager secret named by the MIAMMATCH_SECRET_ID environment
        /// variable as the highest-precedence configuration source. The secret holds a JSON
        /// object shaped like appsettings.json, for example
        /// {"Jwt":{"Key":"..."},"ConnectionStrings":{"ShoppingListDb":"..."}}.
        /// Does nothing when the variable is unset, so local development and tests keep
        /// reading appsettings and user secrets. When it is set but the secret cannot be
        /// read, startup fails rather than silently falling back to plaintext settings.
        /// </summary>
        public static IConfigurationBuilder AddMiamMatchSecrets(this IConfigurationBuilder builder)
        {
            var secretId = Environment.GetEnvironmentVariable(SecretIdVariable);
            if (string.IsNullOrWhiteSpace(secretId))
            {
                return builder;
            }

            using var client = new AmazonSecretsManagerClient();
            // Configuration sources are assembled synchronously, before any host exists to await on.
            var secret = client.GetSecretValueAsync(new GetSecretValueRequest { SecretId = secretId })
                .GetAwaiter()
                .GetResult();

            if (string.IsNullOrWhiteSpace(secret.SecretString))
            {
                throw new InvalidOperationException(
                    $"Secret '{secretId}' holds no string value; it must contain JSON configuration.");
            }

            // Read when the configuration is built, so the stream must outlive this method.
            return builder.AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(secret.SecretString)));
        }
    }
}
