namespace App.Models
{
    public record OpenIDModel
    {
        public string authorization_endpoint;
        public string token_endpoint;
        public string userinfo_endpoint;
    }
}