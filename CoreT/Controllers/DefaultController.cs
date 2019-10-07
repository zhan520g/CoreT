﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreT.Common;
using CoreT.Entity;
using CoreT.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreT.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Policy = "Permission")]
    public class DefaultController : ControllerBase
    {
        private IIdentityUserServices identityUserServices;


        public DefaultController(IIdentityUserServices identityUserServices)
        {
            this.identityUserServices = identityUserServices;
        }

        [HttpGet]
        public async Task<MessageModel<PageModel<IdentityUser>>> Index(int page = 1,string key = "")
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
            {
                key = "";
            }
            int intPageSize = 50;

            var data = await identityUserServices.QueryPage(a => a.Name!=null,page, intPageSize);

            return new MessageModel<PageModel<IdentityUser>>()
            {
                msg = "获取成功",
                success = data.dataCount >= 0,
                response = data
            };
        }
    }
}