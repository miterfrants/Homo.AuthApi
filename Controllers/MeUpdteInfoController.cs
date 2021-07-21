using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Homo.Core.Constants;
using Homo.Core.Helpers;

namespace Homo.AuthApi
{
    [Route("v1/me")]
    [AuthorizeFactory]
    public class MeUpdateInfoController : ControllerBase
    {

        private readonly DBContext _dbContext;
        private readonly string _jwtKey;
        public MeUpdateInfoController(DBContext dbContext, IOptions<AppSettings> appSettings, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
        {
            Secrets secrets = (Secrets)appSettings.Value.Secrets;
            Common common = (Common)appSettings.Value.Common;
            _jwtKey = secrets.JwtKey;
            _dbContext = dbContext;
        }

        [HttpPatch]
        public dynamic updateInfo([FromBody] DTOs.UpdateMe dto, DTOs.JwtExtraPayload extraPayload)
        {
            return new { status = CUSTOM_RESPONSE.OK };
        }
    }
}
