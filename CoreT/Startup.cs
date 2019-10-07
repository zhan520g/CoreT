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
            #region  ��Ȩ����
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
                //3��Use Jwt bearer 
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
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = Configuration.GetSection("SwaggerDoc:Version").Value,
                    Title = Configuration.GetSection("SwaggerDoc:Title").Value,
                    Description = Configuration.GetSection("SwaggerDoc:Description").Value,
                });

                //Add comments description
                var basePath = Environment.CurrentDirectory;
                var xmlPath = Path.Combine(basePath, "CoreT.xml");
                options.IncludeXmlComments(xmlPath);
                options.DocumentFilter<HiddenApiFilter>(); // �ڽӿ��ࡢ����������� [HiddenApi]��������ֹ��Swagger�ĵ�������

                options.OperationFilter<SecurityRequirementsOperationFilter>(); //����
                //��api���token����֤��
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "JWT��Ȩ(���ݽ�������ͷ�н��д���) ֱ�����¿�������Bearer {token}��ע������֮����һ���ո�\"",
                    Name = "Authorization",//jwtĬ�ϵĲ�������
                    In = ParameterLocation.Header,//jwtĬ�ϴ��Authorization��Ϣ��λ��(����ͷ��)
                    Type = SecuritySchemeType.ApiKey
                });
            });
            #endregion

            #region redis����
            services.AddDistributedRedisCache(r =>
            {
                r.Configuration = Configuration["AppSettings:RedisCaching:ConnectionString"];
            });
            #endregion
            services.AddControllers();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            ////�������ע���ϵ
            builder.RegisterModule(new AutofacModuleRegister(Configuration));
            var controllerBaseType = typeof(ControllerBase);
            //�ڿ�������ʹ������ע��
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

          
            //ʹ��Swagger
            app.UseSwagger();
            app.UseSwaggerUI(s =>
            {
                //�汾����
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
