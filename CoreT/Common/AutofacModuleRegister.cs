﻿using Autofac;
using Autofac.Extras.DynamicProxy;
using CoreT.AOP;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CoreT.Common
{
    public class AutofacModuleRegister : Autofac.Module
    {
        public IConfiguration Configuration { get; }

        public AutofacModuleRegister(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        //重写Autofac管道Load方法，在这里注册注入
        protected override void Load(ContainerBuilder builder)
        {
            #region AutoFac DI
            #region 带有接口层的服务注入

            #region Service.dll 注入，有对应接口
            try
            {
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                string servicesDllFile =basePath+Configuration["Assembly:Services"];
                var assemblysServices = Assembly.LoadFrom(servicesDllFile);//直接采用加载文件的方法 

                //foreach (var implement in assemblysServices.GetTypes())
                //{
                //    Type[] interfaceType = implement.GetInterfaces();
                //    foreach (var service in interfaceType)
                //    {
                      
                //    }
                //}
                // AOP 开关，如果想要打开指定的功能，只需要在 appsettigns.json 对应 true 就行。
                var cacheType = new List<Type>();
                if (Configuration["AppSettings:RedisCaching:Enabled"].ObjToBool())
                {
                    builder.RegisterType<RedisCacheAOP>();    //注册要通过反射创建的组件
                    cacheType.Add(typeof(RedisCacheAOP));
                }
                if (Configuration["AppSettings:MemoryCachingAOP:Enabled"].ObjToBool())
                {
                    builder.RegisterType<CacheAOP>();
                    cacheType.Add(typeof(CacheAOP));
                }
                if (Configuration["AppSettings:LogAOP:Enabled"].ObjToBool())
                {
                    builder.RegisterType<LogAOP>();
                    cacheType.Add(typeof(LogAOP));
                }

                builder.RegisterAssemblyTypes(assemblysServices)
                          .AsImplementedInterfaces()
                          .InstancePerLifetimeScope()
                          .EnableInterfaceInterceptors()//引用Autofac.Extras.DynamicProxy;
                          .InterceptedBy(cacheType.ToArray());//允许将拦截器服务的列表分配给注册。 
                #endregion

            #region Repository.dll 注入，有对应接口
                string repositoryDllFile = basePath+Configuration["Assembly:Repository"];
                var assemblysRepository = Assembly.LoadFrom(repositoryDllFile);
                builder.RegisterAssemblyTypes(assemblysRepository).AsImplementedInterfaces();
                #endregion
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\n" + ex.InnerException);
            }

            #endregion

            //DI handler process function
            builder.RegisterType<PolicyHandler>().As<IAuthorizationHandler>();
            builder.RegisterType<Microsoft.AspNetCore.Http.HttpContextAccessor>().As<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            #endregion

        }
    }
}
