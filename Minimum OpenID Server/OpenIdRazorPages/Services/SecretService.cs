using Microsoft.Extensions.Configuration;

namespace OpenIDAppRazor.Services
{
    public class SecretService
    {
        private readonly IConfiguration configuration;

        public SecretService(
            IConfiguration configuration
        )
        {
            this.configuration = configuration;
        }

        public string Get(string key) 
        {
            return configuration.GetValue<string>(key);
        }

    }
}