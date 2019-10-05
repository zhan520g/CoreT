using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using CoreT.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace CoreT
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region  授权服务
            string issuer = Configuration["Jwt:Issuer"];
            string audience = Configuration["Jwt:Audience"];
            string expire = Configuration["Jwt:ExpireMinutes"];
            TimeSpan expiration = TimeSpan.FromMinutes(Convert.ToDouble(expire));
            SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:SecurityKey"]));
            services.AddAuthorizationCore(options =>
            {
                options.AddPolicy("Permission",
                   policy => policy.Requirements.Add(new PolicyRequirement()));
            }).AddAuthentication(s =>
            {
                s.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                s.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                s.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(s =>
            {
                //3、Use Jwt bearer 
                s.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = key,
                    ClockSkew = expiration,
                    ValidateLifetime = true
                };
                s.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        //Token expired
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });
           
            #endregion

            #region Swagger UI
            services.AddSwaggerGen(options =>
            {
                string contactName = Configuration.GetSection("SwaggerDoc:ContactName").Value;
                string contactNameEmail = Configuration.GetSection("SwaggerDoc:ContactEmail").Value;
                string contactUrl = Configuration.GetSection("SwaggerDoc:ContactUrl").Value;
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = Configuration.GetSection("SwaggerDoc:Version").Value,
                    Title = Configuration.GetSection("SwaggerDoc:Title").Value,
                    Description = Configuration.GetSection("SwaggerDoc:Description").Value,
                    Contact = new OpenApiContact { Name = contactName, Email = contactNameEmail, Url = new Uri(contactUrl) },
                    License = new OpenApiLicense { Name = contactName, Url = new Uri(contactUrl) }
                });

                //Add comments description
                var basePath = Path.GetDirectoryName(AppContext.BaseDirectory);//get application located directory
                var xmlPath = Path.Combine(basePath, "CoreT.xml");
                options.IncludeXmlComments(xmlPath);
                options.DocumentFilter<HiddenApiFilter>(); // 在接口类、方法标记属性 [HiddenApi]，可以阻止【Swagger文档】生成
                options.OperationFilter<AddHeaderOperationFilter>("correlationId", "Correlation Id for the request", false); // adds any string you like to the request headers - in this case, a correlation id
                options.OperationFilter<AddResponseHeadersFilter>();
                options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                options.OperationFilter<SecurityRequirementsOperationFilter>();
                //给api添加token令牌证书
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "JWT授权(数据将在请求头中进行传输) 直接在下框中输入Bearer {token}（注意两者之间是一个空格）\"",
                    Name = "Authorization",//jwt默认的参数名称
                    In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                    Type = SecuritySchemeType.ApiKey
                });
            });

            #endregion

            #region redis服务
            services.AddDistributedRedisCache(r =>
            {
                r.Configuration = Configuration["AppSettings:RedisCaching:ConnectionString"];
            });
            #endregion
            services.AddControllers();
        }


        public void ConfigureContainer(ContainerBuilder builder)
        {
            ////添加依赖注入关系
            builder.RegisterModule(new AutofacModuleRegister(Configuration));
            var controllerBaseType = typeof(ControllerBase);
            //在控制器中使用依赖注入
            builder.RegisterAssemblyTypes(typeof(Program).Assembly)
                .Where(t => controllerBaseType.IsAssignableFrom(t) && t != controllerBaseType)
                .PropertiesAutowired();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
          
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            #region MiniProfiler
            app.UseMiniProfiler();
            #endregion

            app.UseRouting();

            //Enable Authentication
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers().RequireAuthorization(); ;
            });

            //使用Swagger
            app.UseSwagger();
            app.UseSwaggerUI(s =>
            {
                //版本控制
                s.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreT API V1");
                //foreach (var description in provider.ApiVersionDescriptions.Reverse())
                //{
                //    s.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                //        $"momo API {description.GroupName.ToUpperInvariant()}");
                //}
            });



        }
    }
}
