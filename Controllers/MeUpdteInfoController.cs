using Microsoft.AspNetCore.Mvc;
using Homo.Core.Constants;

namespace Homo.AuthApi
{
    [Route("v1/me")]
    [AuthorizeFactory]
    public class MeUpdateInfoController : ControllerBase
    {

        private readonly DBContext _dbContext;
        public MeUpdateInfoController(DBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPatch]
        public dynamic updateInfo([FromBody] DTOs.UpdateMe dto, DTOs.JwtExtraPayload extraPayload)
        {
            UserDataservice.Update(_dbContext, extraPayload.Id, dto, extraPayload.Id);
            return new { status = CUSTOM_RESPONSE.OK };
        }
    }
}
