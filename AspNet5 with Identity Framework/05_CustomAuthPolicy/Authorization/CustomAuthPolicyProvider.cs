using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using App.RequiredClaims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using MimeKit.IO.Filters;

namespace App.PolicyProvider
{
    public static class CustomAuthorizationPolicyFactory
    {
        public static AuthorizationPolicy Create(string policyName)
        {
            var parts = policyName.Split('.');
            var type = parts.First();
            var value = parts.Last();
            if(!policyName.Contains('.')) 
            return new AuthorizationPolicyBuilder().AddRequirements(new CustomRequireClaim(type)).Build();
            //if(!policyName.Contains('.')) return new AuthorizationPolicyBuilder().RequireClaim(type).Build();
            return new AuthorizationPolicyBuilder().RequireCustomClaim(type,value).Build();
        }
    }

    public class CustomAuthPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public CustomAuthPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
            
        }

        //PolicyName.Value
        public override Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {            
            var policy = CustomAuthorizationPolicyFactory.Create(policyName);
            return Task.FromResult(policy);            
        }
      
        
    }
        
 
}