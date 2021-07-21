using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Homo.Core.Constants;
using Homo.Core.Helpers;

namespace Homo.AuthApi
{
    [Route("v1/me/reset-password")]
    [AuthorizeFactory]
    public class MeResetPasswordController : ControllerBase
    {

        private readonly DBContext _dbContext;
        private readonly string _jwtKey;
        public MeResetPasswordController(DBContext dbContext, IOptions<AppSettings> appSettings, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            Secrets secrets = (Secrets)appSettings.Value.Secrets;
            Common common = (Common)appSettings.Value.Common;
            _jwtKey = secrets.JwtKey;
            _dbContext = dbContext;
        }

        [HttpPatch]
        public dynamic resetPassword([FromBody] DTOs.ResetMyPassword dto, dynamic extraPayload)
        {
            long userId = (long)extraPayload.userId.Value;
            User user = UserDataservice.GetOne(_dbContext, userId, true);
            bool isEmailAccount = user.Hash != null && user.Hash.Length > 0;

            if (isEmailAccount && (dto.OlderPassword == null || dto.OlderPassword.Length == 0))
            {
                throw new CustomException(ERROR_CODE.WRONG_PASSWORD, HttpStatusCode.BadRequest);
            }

            if (isEmailAccount && user.Hash != CryptographicHelper.GenerateSaltedHash(dto.OlderPassword, user.Salt))
            {
                throw new CustomException(ERROR_CODE.WRONG_PASSWORD, HttpStatusCode.BadRequest);
            }

            string salt = CryptographicHelper.GetSalt(64);
            string hash = CryptographicHelper.GenerateSaltedHash(dto.Password, salt);
            UserDataservice.ResetPassword(_dbContext, userId, salt, hash);
            return new { status = CUSTOM_RESPONSE.OK };
        }
    }
}
