using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Homo.AuthApi
{
    public enum AUTH_TYPE
    {
        SIGN_UP,
        COMMON,
        ANONYMOUS
    }
    public class AuthorizeFactory : ActionFilterAttribute, IFilterFactory
    {
        public bool IsReusable => true;
        public AUTH_TYPE type = AUTH_TYPE.COMMON;
        public AuthorizeFactory(AUTH_TYPE type = AUTH_TYPE.COMMON)
        {
            this.type = type;
        }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            IOptions<AppSettings> config = serviceProvider.GetService<IOptions<AppSettings>>();
            var secrets = (Secrets)config.Value.Secrets;
            AuthorizeAttribute attribute = new AuthorizeAttribute();
            if (this.type == AUTH_TYPE.COMMON)
            {
                attribute._jwtKey = secrets.JwtKey;
            }
            else if (this.type == AUTH_TYPE.SIGN_UP)
            {
                attribute.isSignUp = true;
                attribute._jwtKey = secrets.SignUpJwtKey;
            }
            else if (this.type == AUTH_TYPE.ANONYMOUS)
            {
                attribute.isAnonymous = true;
                attribute._jwtKey = secrets.AnonymousJwtKey;
            }

            return attribute;
        }
    }
}