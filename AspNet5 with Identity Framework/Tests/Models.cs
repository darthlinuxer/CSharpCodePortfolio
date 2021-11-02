using Microsoft.AspNetCore.Identity;

namespace Tests.Models
{
    public class ReturnedValue { public string? Value { get; set; } };
    public class ReturnedToken { public string? Id_Token { get; set; } }
    public class ReturnedAccessToken { public string? Access_Token { get; set; } }
    public class ReturnedMessage { public string? Msg { get; set; } }
    public class ReturnedMsgAndUser
    {
        public string? Msg { get; set; }
        public IdentityUser? User {get; set;}
    }
}