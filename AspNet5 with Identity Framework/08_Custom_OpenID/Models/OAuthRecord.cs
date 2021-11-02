namespace OpenIDApp.Models
{
    public record OAuthRecord
    {
        public string Oauth_client_id {get; init;}
        public string Oauth_client_secret {get; init;}
    }
}