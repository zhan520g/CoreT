using CoreT.Common;
using CoreT.Entity;
using CoreT.Entity.ViewModels;
using CoreT.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CoreT.Services
{
    /// <summary>
    /// JSON WEB TOKEN
    /// </summary>
    public class JwtAppService : IJwtAppService
    {
        #region Initialize

        /// <summary>
        /// 已授权的 Token 信息集合
        /// </summary>
        private static ISet<JwtAuthorizationDto> _tokens = new HashSet<JwtAuthorizationDto>();

        /// <summary>
        /// 分布式缓存
        /// </summary>
        private readonly IDistributedCache _cache;

        /// <summary>
        /// 配置信息
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 获取 HTTP 请求上下文
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;


        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        public JwtAppService(IDistributedCache cache, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }


        #endregion


        #region API Implements

        /// <summary>
        ///  Token 创建
        ///  Claim（每一项的证件信息）=》ClaimsIdentity（证件）=》ClaimsPrincipal（证件持有者）
        /// </summary>
        /// <param name="dto">用户信息数据传输对象</param>
        /// <returns></returns>
        public JwtAuthorizationDto Create(IdentityUser dto)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecurityKey"]));

            DateTime authTime = DateTime.Now;
            DateTime expiresAt = authTime.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"]));

            //将用户信息添加到 Claim 中,制作身份证
            var identity = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme);

            //添加用户信息
            IEnumerable<Claim> claims = new Claim[] {
              new Claim(ClaimTypes.Name,dto.Name),
              new Claim(ClaimTypes.Role,dto.Salt.ToString()),
              new Claim(ClaimTypes.Email,dto.Password),
              new Claim(ClaimTypes.Expiration,expiresAt.ToString())
            };
            //身份证添加信息
            identity.AddClaims(claims);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc =DateTime.Now.AddHours(24),//24
            };

            // 第一步 根据配置信息和用户信息创建一个 token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),//创建声明信息
                Issuer = _configuration["Jwt:Issuer"],//Jwt token 的签发者
                Audience = _configuration["Jwt:Audience"],//Jwt token 的接收者
                Expires = expiresAt,//过期时间
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)//创建 token,使用的hash算法，如：HMAC SHA256或RSA
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            // 第二步 将加密后的用户信息写入到 HttpContext 上下文中, 先签发一个加密后的用户信息凭证，用来标识用户的身份
            var principal = new ClaimsPrincipal(identity);
            _httpContextAccessor.HttpContext.SignInAsync(JwtBearerDefaults.AuthenticationScheme, principal, authProperties);


            //第三步 将创建好的 token 信息添加到静态的 HashSet<JwtAuthorizationDto> 集合中。
            var jwt = new JwtAuthorizationDto
            {
                UserId = dto.Id,
                Token = tokenHandler.WriteToken(token),
                Auths = new DateTimeOffset(authTime).ToUnixTimeSeconds(),
                Expires = new DateTimeOffset(expiresAt).ToUnixTimeSeconds(),
                Success = true
            };
            _tokens.Add(jwt);

            return jwt;
        }


        /// <summary>
        /// token刷新
        /// </summary>
        /// <param name="token"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task<JwtAuthorizationDto> RefreshAsync(string token, IdentityUser dto)
        {
            var jwtOld = GetExistenceToken(token);
            if (jwtOld == null)
            {
                return new JwtAuthorizationDto()
                {
                    Token = "未获取到当前 Token 信息",
                    Success = false
                };
            }

            var jwt = Create(dto);

            //停用修改前的 Token 信息
            await DeactivateCurrentAsync();

            return jwt;
        }


        /// <summary>
        /// 判断是否存在当前 Token
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns></returns>
        public JwtAuthorizationDto GetExistenceToken(string token)
        => _tokens.SingleOrDefault(x => x.Token == token);

        /// <summary>
        /// Token停用
        /// 将停用的 token 信息添加到 Redis 缓存中，之后，在用户请求时判断这个 token 是不是存在于 Redis 中。
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns></returns>
        public async Task DeactivateAsync(string token)
        => await _cache.SetStringAsync(GetKey(token),
                "yes", new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"]))
                });

        /// <summary>
        /// 停用当前 Token
        /// </summary>
        /// <returns></returns>
        public async Task DeactivateCurrentAsync()
        => await DeactivateAsync(GetCurrentAsync());

        /// <summary>
        /// 设置缓存中过期 Token 值的 key
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns></returns>
        private static string GetKey(string token)
            => $"deactivated token:{token}";

        /// <summary>
        /// 获取 HTTP 请求的 Token 值
        /// </summary>
        /// <returns></returns>
        private string GetCurrentAsync()
        {
            //http header
            var authorizationHeader = _httpContextAccessor
                .HttpContext.Request.Headers["authorization"];

            //token
            return authorizationHeader == StringValues.Empty
                ? string.Empty
                : authorizationHeader.Single().Split(" ").Last();// bearer tokenvalue
        }

        /// <summary>
        /// token是否有效
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> IsActiveAsync(string token)
        {
            var value= await _cache.GetAsync(token);
            if (value != null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// 当前token是否有效
        /// </summary>
        /// <returns></returns>
        public Task<bool> IsCurrentActiveTokenAsync()
        {
            var token = GetCurrentAsync();
            return IsActiveAsync(token);
        }

        #endregion
    }
}
