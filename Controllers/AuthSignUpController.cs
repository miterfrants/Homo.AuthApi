﻿using System;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Homo.Core.Constants;
using Homo.Core.Helpers;

namespace Homo.AuthApi
{
    [Route("v1/auth")]
    [Homo.Api.Validate]
    public class AuthSignUpController : ControllerBase
    {
        private readonly Homo.Api.CommonLocalizer _commonLocalizer;
        private readonly DBContext _dbContext;
        private readonly string _jwtKey;
        private readonly int _jwtExpirationMonth;
        private readonly string _envName;
        private readonly string _sendGridAPIKey;
        private readonly string _systemEmail;
        private readonly string _websiteEndpoint;
        private readonly string _fbAppId;
        private readonly string _googleClientId;
        private readonly string _lineClientId;
        private readonly string _fbClientSecret;
        private readonly string _googleClientSecret;
        private readonly string _lineClientSecret;
        private readonly bool _authByCookie;
        public AuthSignUpController(DBContext dbContext, IOptions<AppSettings> appSettings, Microsoft.AspNetCore.Hosting.IWebHostEnvironment env, Homo.Api.CommonLocalizer commonLocalizer)
        {
            Secrets secrets = (Secrets)appSettings.Value.Secrets;
            Common common = (Common)appSettings.Value.Common;
            _commonLocalizer = commonLocalizer;
            _jwtKey = secrets.SignUpJwtKey;
            _jwtExpirationMonth = common.JwtExpirationMonth;
            _dbContext = dbContext;
            _envName = env.EnvironmentName;
            _sendGridAPIKey = secrets.SendGridApiKey;
            _systemEmail = common.SystemEmail;
            _websiteEndpoint = common.WebSiteEndpoint;
            _fbAppId = common.FbAppId;
            _fbClientSecret = secrets.FbClientSecret;
            _googleClientId = common.GoogleClientId;
            _googleClientSecret = secrets.GoogleClientSecret;
            _lineClientId = common.LineClientId;
            _lineClientSecret = secrets.LineClientSecret;
            _authByCookie = common.AuthByCookie;
        }

        [Route("sign-up")]
        [AuthorizeFactory(AUTH_TYPE.SIGN_UP)]
        [HttpPost]
        public ActionResult<dynamic> signUp([FromBody] DTOs.SignUp dto, dynamic extraPayload)
        {
            User newUser = null;
            User user = null;
            SocialMediaProvider? socialMediaProvider = null;
            string sub = null;
            if (extraPayload.facebookSub != null)
            {
                socialMediaProvider = SocialMediaProvider.FACEBOOK;
                sub = extraPayload.facebookSub.Value;
            }
            else if (extraPayload.googleSub != null)
            {
                socialMediaProvider = SocialMediaProvider.GOOGLE;
                sub = extraPayload.googleSub.Value;
            }
            else if (extraPayload.lineSub != null)
            {
                socialMediaProvider = SocialMediaProvider.LINE;
                sub = extraPayload.lineSub.Value;
            }

            if (socialMediaProvider == null)
            {
                user = UserDataservice.GetOneBySocialMediaSub(_dbContext, socialMediaProvider.GetValueOrDefault(), sub);
            }

            if (user != null)
            {
                throw new CustomException(ERROR_CODE.SIGN_IN_BY_OTHER_WAY, HttpStatusCode.BadRequest, null, new Dictionary<string, dynamic>(){
                            {"duplicatedUserProvider", AuthHelper.GetDuplicatedUserType(user)}
                        });
            }

            if (sub != null)
            {
                newUser = UserDataservice.SignUpWithSocialMedia(_dbContext, socialMediaProvider.GetValueOrDefault(), extraPayload.facebookSub.Value, extraPayload.email.Value, extraPayload.name.Value, extraPayload.profile.Value, dto.FirstName, dto.LastName, dto.Birthday);
            }
            else
            {
                string salt = CryptographicHelper.GetSalt(64);
                string hash = CryptographicHelper.GenerateSaltedHash(dto.Password, salt);
                newUser = UserDataservice.SignUp(_dbContext, extraPayload.email.Value, dto.Password, dto.FirstName, dto.LastName, salt, hash, dto.Birthday);
            }

            var userPayload = new
            {
                id = newUser.Id,
                email = newUser.Email,
                firstname = newUser.FirstName,
                lastname = newUser.LastName,
                profile = newUser.Profile
            };
            string token = JWTHelper.GenerateToken(_jwtKey, _jwtExpirationMonth * 30 * 24 * 60, userPayload);

            if (_authByCookie)
            {
                Response.Cookies.Append("token", token, AuthHelper.GetSecureCookieOptions());
            }
            else
            {
                return new
                {
                    id = newUser.Id,
                    email = newUser.Email,
                    firstname = newUser.FirstName,
                    lastname = newUser.LastName,
                    profile = newUser.Profile,
                    token = token
                };
            }

            return userPayload;
        }

        [Route("send-verify-email")]
        [HttpPost]
        public async Task<dynamic> sendVerifyEmail([FromBody] DTOs.SendValidatedEmail dto)
        {
            string ip = NetworkHelper.GetIpFromRequest(Request);
            int countByIp = VerifyCodeDataservice.GetTodayCountByIp(_dbContext, ip);
            if (countByIp > 10)
            {
                throw new CustomException(ERROR_CODE.TOO_MANY_TIMES_TO_SEND_MAIL, HttpStatusCode.Forbidden);
            }

            int countByEmail = VerifyCodeDataservice.GetTodayCountByEmail(_dbContext, dto.Email);
            if (countByEmail > 10)
            {
                throw new CustomException(ERROR_CODE.TOO_MANY_TIMES_TO_SEND_MAIL, HttpStatusCode.Forbidden);
            }

            User user = UserDataservice.GetOneByEmail(_dbContext, dto.Email);
            List<string> duplicatedUserProvider = new List<string>();
            if (user != null)
            {
                throw new CustomException(ERROR_CODE.SIGN_IN_BY_OTHER_WAY, HttpStatusCode.BadRequest, null, new Dictionary<string, dynamic>(){
                            {"duplicatedUserProvider", AuthHelper.GetDuplicatedUserType(user)}
                        });
            }

            string code = CryptographicHelper.GetSpecificLengthRandomString(6, true, true);
            VerifyCodeDataservice.Create(_dbContext, new DTOs.VerifyCode()
            {
                Email = dto.Email,
                Code = code,
                Expiration = DateTime.Now.AddSeconds(3 * 60),
                Ip = ip
            });

            await MailHelper.Send(MailProvider.SEND_GRID, new MailTemplate()
            {
                Subject = _commonLocalizer.Get("verify email"),
                Content = _commonLocalizer.Get("verify link", null, new Dictionary<string, string>() {
                    { "link", $"{_websiteEndpoint}/auth/verify-email" },
                    { "code", code }
                })
            }, _systemEmail, dto.Email, _sendGridAPIKey);
            return new { status = CUSTOM_RESPONSE.OK };
        }

        [Route("verify-email")]
        [HttpPost]
        public dynamic verifyEmail([FromBody] DTOs.VerifyEmail dto)
        {
            VerifyCode record = VerifyCodeDataservice.GetOneUnUsedByEmail(_dbContext, dto.Email, dto.Code);
            if (record == null)
            {
                throw new CustomException(ERROR_CODE.VERIFY_CODE_NOT_FOUND, HttpStatusCode.NotFound);
            }
            record.IsUsed = true;
            _dbContext.SaveChanges();
            return new { token = JWTHelper.GenerateToken(_jwtKey, 5, new { email = record.Email }) };
        }

        [Route("auth-with-social-media")]
        [HttpPost]
        public async Task<dynamic> authWithSocialMedia([FromBody] DTOs.AuthWithSocialMedia dto)
        {
            oAuthResp authResp = null;
            UserInfo userInfo = null;
            User user = null;
            SocialMediaProvider? provider = null;

            try
            {
                if (dto.Provider == SocialMediaProvider.FACEBOOK)
                {
                    authResp = await FacebookOAuthHelper.GetAccessToken(_fbAppId, dto.RedirectUri, _fbClientSecret, dto.Code);
                    userInfo = await FacebookOAuthHelper.GetUserInfo(authResp.access_token);
                    provider = SocialMediaProvider.FACEBOOK;
                }
                else if (dto.Provider == SocialMediaProvider.GOOGLE)
                {
                    authResp = await GoogleOAuthHelper.GetAccessToken(_googleClientId, dto.RedirectUri, _googleClientSecret, dto.Code);
                    userInfo = await GoogleOAuthHelper.GetUserInfo(authResp.access_token);
                    provider = SocialMediaProvider.GOOGLE;
                }
                else if (dto.Provider == SocialMediaProvider.LINE)
                {
                    authResp = await LineOAuthHelper.GetAccessToken(_lineClientId, dto.RedirectUri, _lineClientSecret, dto.Code);
                    userInfo = await LineOAuthHelper.GetUserInfo(authResp.access_token);
                    provider = SocialMediaProvider.LINE;
                }
                user = UserDataservice.GetOneBySocialMediaSub(_dbContext, provider.GetValueOrDefault(), userInfo.sub);
            }
            catch (System.Exception)
            {
                throw new CustomException(ERROR_CODE.INVALID_CODE_FROM_SOCIAL_MEDIA, HttpStatusCode.BadRequest);
            }

            if (user != null)
            {
                throw new CustomException(ERROR_CODE.SIGN_IN_BY_OTHER_WAY, HttpStatusCode.BadRequest, null, new Dictionary<string, dynamic>(){
                            {"duplicatedUserProvider", AuthHelper.GetDuplicatedUserType(user)}
                        });
            }

            List<string> duplicatedUserProvider = new List<string>();
            user = UserDataservice.GetOneByEmail(_dbContext, userInfo.email);
            if (user != null)
            {
                throw new CustomException(ERROR_CODE.SIGN_IN_BY_OTHER_WAY, HttpStatusCode.BadRequest, null, new Dictionary<string, dynamic>(){
                            {"duplicatedUserProvider", AuthHelper.GetDuplicatedUserType(user)}
                        });
            }
            string token = "";
            if (dto.Provider == SocialMediaProvider.FACEBOOK)
            {
                token = JWTHelper.GenerateToken(_jwtKey, 5, new { email = userInfo.email, facebookSub = userInfo.sub, name = userInfo.name, profile = userInfo.picture });
            }
            else if (dto.Provider == SocialMediaProvider.GOOGLE)
            {
                token = JWTHelper.GenerateToken(_jwtKey, 5, new { email = userInfo.email, googleSub = userInfo.sub, name = userInfo.name, profile = userInfo.picture });
            }
            else if (dto.Provider == SocialMediaProvider.LINE)
            {
                token = JWTHelper.GenerateToken(_jwtKey, 5, new { email = userInfo.email, lineSub = userInfo.sub, name = userInfo.name, profile = userInfo.picture });
            }

            return new { token = token };
        }


    }
}