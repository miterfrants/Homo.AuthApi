using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using System.Net;
using Homo.Core.Constants;

namespace Homo.AuthApi
{
    public class AuthorizeAttribute : ActionFilterAttribute
    {

        public string _jwtKey { get; set; }
        public bool isAnonymous { get; set; }
        public bool isSignUp { get; set; }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string token = null;
            context.HttpContext.Request.Cookies.TryGetValue("token", out token);
            if (token == null || token == "")
            {
                string authorization = context.HttpContext.Request.Headers["Authorization"];
                token = authorization.Substring("Bearer ".Length).Trim();
            }

            if (token == null || token == "")
            {
                throw new CustomException(ERROR_CODE.UNAUTH_ACCESS_API, HttpStatusCode.Unauthorized);
            }

            if (JWTHelper.isExpired(token))
            {
                throw new CustomException(ERROR_CODE.TOKEN_EXPIRED, HttpStatusCode.Unauthorized);
            }
            ClaimsPrincipal payload = JWTHelper.GetPayload(_jwtKey, token);
            if (payload == null)
            {
                throw new CustomException(ERROR_CODE.UNAUTH_ACCESS_API, HttpStatusCode.Unauthorized);
            }

            long? userId = JWTHelper.GetUserIdFromRequest(_jwtKey, context.HttpContext.Request);

            if (userId == null && isSignUp == false)
            {
                throw new CustomException(ERROR_CODE.USER_NOT_FOUND, HttpStatusCode.NotFound);
            }
            // pass extraPayload to controller function
            context.ActionArguments["extraPayload"] = Newtonsoft.Json.JsonConvert.DeserializeObject<DTOs.JwtExtraPayload>(payload.FindFirstValue("extra"));
        }
    }
}