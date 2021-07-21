using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Homo.Core.Constants;
using Homo.Core.Helpers;

namespace Homo.AuthApi
{
    [Route("v1/me")]
    [AuthorizeFactory]
    public class MeVerifyPhoneController : ControllerBase
    {

        private readonly DBContext _dbContext;
        private readonly string _jwtKey;
        public MeVerifyPhoneController(DBContext dbContext, IOptions<AppSettings> appSettings, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            Secrets secrets = (Secrets)appSettings.Value.Secrets;
            Common common = (Common)appSettings.Value.Common;
            _jwtKey = secrets.JwtKey;
            _dbContext = dbContext;
        }

        [HttpPost]
        [Route("send-sms")]
        public dynamic sendSms([FromBody] DTOs.ResetMyPassword dto, DTOs.JwtExtraPayload extraPayload)
        {

            return new { status = CUSTOM_RESPONSE.OK };
        }
    }
}
