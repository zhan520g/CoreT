using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            #region 带有接口层的服务注入
            string assemblies = Configuration["Assembly:dll"];
            if (!string.IsNullOrEmpty(assemblies))
            {
                foreach (var item in assemblies.Split('|'))
                {
                    Assembly assembly = Assembly.Load(item);
                    foreach (var implement in assembly.GetTypes())
                    {
                        if (implement.FullName.Contains("BaseRepository")||
                            implement.FullName.Contains("BaseServices")) continue;

                        Type[] interfaceType = implement.GetInterfaces();
                        foreach (var service in interfaceType)
                        {
                            services.AddTransient(service, implement);
                        }
                    }
                } 
            }
            #endregion
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
