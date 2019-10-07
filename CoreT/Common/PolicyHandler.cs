using CoreT.IServices;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CoreT.Common
{
    public class PolicyHandler : AuthorizationHandler<PolicyRequirement>
    {

        /// <summary>
        /// 授权方式（cookie, bearer, oauth, openid）
        /// </summary>
        public IAuthenticationSchemeProvider Schemes { get; set; }

        /// <summary>
        /// jwt 服务
        /// </summary>
        private readonly IJwtAppService _jwtApp;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="schemes"></param>
        /// <param name="jwtApp"></param>
        public PolicyHandler(IAuthenticationSchemeProvider schemes, IJwtAppService jwtApp)
        {
            Schemes = schemes;
            _jwtApp = jwtApp;
        }
        /// <summary>
        /// //授权处理 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="requirement"></param>
        /// <returns></returns>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PolicyRequirement requirement)
        {
            //Todo：获取角色、Url 对应关系
            List<Menu> list = new List<Menu> {
                new Menu
                {
                    Role = Guid.Empty.ToString(),
                    Url = "/api/v1.0/Values"
                },
                new Menu
                {
                    Role=Guid.Empty.ToString(),
                    Url="/api/v1.0/secret/deactivate"
                },
                new Menu
                {
                    Role=Guid.Empty.ToString(),
                    Url="/api/v1.0/secret/refresh"
                }
            };

            var http = (context.Resource as Microsoft.AspNetCore.Routing.RouteEndpoint);
            var isAuthenticated = context.User.Identity.IsAuthenticated;

            //获取授权方式
            var defaultAuthenticate = await Schemes.GetDefaultAuthenticateSchemeAsync();
            if (defaultAuthenticate != null)
            {
                //验证签发的用户信息
                if (context.User.Identity.IsAuthenticated)
                {
                    //判断是否为已停用的 Token
                    if (await _jwtApp.IsCurrentActiveTokenAsync())
                    {
                        context.Fail();
                        return ;
                    }

                    //判断角色与 Url 是否对应
                    //
                    var url = http.RoutePattern.RequiredValues.GetValueOrDefault("controller") + "/" + http.RoutePattern.RequiredValues.GetValueOrDefault("action");

                    var role = context.User.Claims.Where(c => c.Type == ClaimTypes.Role).FirstOrDefault().Value;
                    var menu = list.Where(x => x.Role.Equals(role) && x.Url.ToLower().Equals(url)).FirstOrDefault();

                    if (menu == null)
                    {
                        context.Fail();
                        return;
                    }

                    //判断是否过期
                    if (DateTime.Parse(context.User.Claims.SingleOrDefault(s => s.Type == ClaimTypes.Expiration).Value) >= DateTime.UtcNow)
                    {
                        //允许此标记通过,并将挂起的状态删除
                        context.Succeed(requirement);
                    }
                    else
                    {
                        context.Fail();
                    }
                    return ;
                }
            }
            context.Fail();
        }

        /// <summary>
        /// 测试菜单类
        /// </summary>
        public class Menu
        {
            public string Role { get; set; }

            public string Url { get; set; }
        }
    }
}
