using App.PolicyProvider;
using Microsoft.AspNetCore.Authorization;

namespace App.Attributes
{
    public class SecurityLevelAttribute : AuthorizeAttribute
    {
        public SecurityLevelAttribute(int level)
        {
            Policy = $"Security.{level}";
        }
    }
}