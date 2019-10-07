using System.Collections.Generic;
using System.Threading.Tasks;
using CoreT.Entity;
using CoreT.Entity.ViewModels;
using CoreT.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.AspNetCore.Authentication;

namespace CoreT.Controllers.v1
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Permission")]
    public class SecretController : ControllerBase
    {
        #region Initialize

        /// <summary>
        /// Jwt 服务
        /// </summary>
        private readonly IJwtAppService _jwtApp;

        /// <summary>
        /// 用户服务
        /// </summary>
        private readonly IIdentityUserServices _userApp;

        /// <summary>
        /// 配置信息
        /// </summary>
        public IConfiguration _configuration { get; }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="jwtApp"></param>
        /// <param name="userApp"></param>
        public SecretController(IConfiguration configuration,
            IJwtAppService jwtApp, IIdentityUserServices userApp)
        {
            _configuration = configuration;
            _jwtApp = jwtApp;
            _userApp = userApp;
        }
        #endregion

        #region API
        /// <summary>
        /// 停用 Jwt 授权数据
        /// </summary>
        /// <returns></returns>
        [HttpPost("deactivate")]
        public async Task<IActionResult> CancelAccessToken()
        {
            await _jwtApp.DeactivateCurrentAsync();
            return Ok();
        }

        /// <summary>
        /// 获取 Jwt 授权数据
        /// </summary>
        /// <param name="dto">授权用户信息</param>
        [HttpPost("token")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync(SecretDto dto)
        {
            var user = await _userApp.QueryFirst(p => p.Name == dto.UserName & p.Password == dto.Password);
            if (user == null)
                return Ok(new JwtResponseDto
                {
                    Access = "用户不存在,无权访问",
                    Type = "Bearer",
                    err_code = 1,
                    Data = new Profile
                    {
                        Name = dto.UserName,
                        Auths = 0,
                        Expires = 0
                    }
                });

            var jwt = _jwtApp.Create(user);

            return Ok(new JwtResponseDto
            {
                Access = jwt.Token,
                Type = "Bearer",
                err_code = 0,
                Data = new Profile
                {
                    Name = user.Name,
                    Auths = jwt.Auths,
                    Expires = jwt.Expires
                }
            });
        }

        /// <summary>
        /// 刷新 Jwt 授权数据
        /// </summary>
        /// <param name="dto">刷新授权用户信息</param>
        /// <returns></returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAccessTokenAsync([FromBody] SecretDto dto)
        {
            var auth = HttpContext.AuthenticateAsync().Result.Principal.Claims;
            var user = await _userApp.QueryFirst(p => p.Name == dto.UserName & p.Password == dto.Password);

            if (user == null)
                return Ok(new JwtResponseDto
                {
                    Access = "无权访问",
                    Type = "Bearer",
                    err_code = 1,
                    Data = new Profile
                    {
                        Name = dto.UserName,
                        Auths = 0,
                        Expires = 0
                    }
                });

            var jwt =await _jwtApp.RefreshAsync(dto.Token, user);

            return Ok(new JwtResponseDto
            {
                Access = jwt.Token,
                err_code = 0,
                Type = "Bearer",
                Data = new Profile
                {
                    Name = user.Name,
                    Auths = jwt.Success ? jwt.Auths : 0,
                    Expires = jwt.Success ? jwt.Expires : 0
                }
            });
        }
       
        #endregion
    }
}