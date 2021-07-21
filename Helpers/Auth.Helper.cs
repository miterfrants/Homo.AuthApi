using System.Collections.Generic;

namespace Homo.AuthApi
{
    public static class AuthHelper
    {
        public static Dictionary<string, string> GetDuplicatedUserType(User user)
        {
            Dictionary<string, string> duplicatedUserList = new Dictionary<string, string>();
            if (user.FbSub != null)
            {
                duplicatedUserList.Add("facebook", user.Email);
            }
            if (user.LineSub != null)
            {
                duplicatedUserList.Add("google", user.Email);
            }
            if (user.FbSub == null && user.LineSub == null)
            {
                duplicatedUserList.Add("origin", user.Email);
            }
            return duplicatedUserList;
        }

        public static Microsoft.AspNetCore.Http.CookieOptions GetSecureCookieOptions()
        {
            return new Microsoft.AspNetCore.Http.CookieOptions() { HttpOnly = true, SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None, Secure = true };
        }
    }
}