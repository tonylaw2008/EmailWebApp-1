using log4net;
using log4net.Config;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.AccessControl; 
using Microsoft.Extensions.Hosting;

namespace MailEnhanceService
{
    //建立配置選項類
    public class EmailSettingOptions
    {
        public List<SenderEmailAccount> SenderEmailAccountList { get; set; } = new();
    }
    public static class ServiceConfigurator
    {
        private static ILoggerRepository? _logRepository;

        public static IConfigurationRoot ReadFromAppSettings()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory) 
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public static IServiceCollection ConfigureServices(IServiceCollection services, AuthenticUserModel authModel)
        { 
            var configuration = ReadFromAppSettings();

            // 注册 AuthenticUserModel 实例
            services.AddSingleton(authModel);

            // Email Sender Account List 配置綁定
            //var emailSettingOptions = new EmailSettingOptions();
            //configuration.GetSection("senderEmailAccountList").Bind(emailSettingOptions.SenderEmailAccountList);
            //// 关键修复：同时注册具体类型和接口类型
            //var accountList = emailSettingOptions.SenderEmailAccountList;
            //services.AddSingleton(accountList); // 注册具体类型 List<SenderEmailAccount>
            //services.AddSingleton<IList<SenderEmailAccount>>(accountList); // 注册接口类型

            // 改為從EmailConfigHelper獲取郵件帳戶列表，並註冊到DI容器中
            var ret = EmailConfigHelper.ToEmailAccountsInstance(authModel.MainComId, out IList<SenderEmailAccount> accountList);
            services.AddSingleton(accountList); // 注册具体类型 List<SenderEmailAccount>
            services.AddSingleton<IList<SenderEmailAccount>>(accountList); // 注册接口类型

            // 配置内存缓存 (這個pagekage有問題，先忽略)
            services.AddMemoryCache(); 
           
            // 配置 log4net
            services = ConfigureLog4Net(services);

            // 注册服务（使用瞬态生命周期）
            authModel.MainComId = authModel.MainComId ?? string.Empty; 
            services.AddSingleton(authModel.MainComId);
            services.AddTransient<EmailAppService>();

            // 注入郵件發送單元
            services.AddTransient<EmailEnhanceHelper>();

            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddLog4Net(); // 使用 log4net
            });
             
            return services;
        }
          
        /// <summary>
        ///  日誌Log4net服務擴展
        /// </summary>
        /// <param name="services"></param>
        private static IServiceCollection ConfigureLog4Net(IServiceCollection services)
        {
            if (_logRepository == null)
            {
                // 正确获取入口程序集
                Assembly entryAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

                // 创建仓库时使用程序集，而不是路径
                _logRepository = LogManager.CreateRepository(
                    entryAssembly,
                    typeof(log4net.Repository.Hierarchy.Hierarchy)
                );

                string baseDir = AppContext.BaseDirectory;
                string configPath = Path.Combine(baseDir, "log4net.config");

                // 验证配置文件是否存在
                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException("log4net.config file not found", configPath);
                }

                XmlConfigurator.Configure(_logRepository, new FileInfo(configPath));
            }

            services.AddLogging(builder => {
                builder.ClearProviders();
                builder.AddLog4Net();
            });

            return services;
        }
         
        //public static IServiceCollection AddMailEnhanceService(this IServiceCollection services,string? mainComId)
        //{
        //    mainComId = mainComId ?? string.Empty;
        //    return ConfigureServices(services, mainComId);
        //}

        // 配置中間件 范例 （未使用2025-8-25）
        public static void  Configure(IApplicationBuilder app, IHostEnvironment hostEnvironment)
        {  
            //test middleware
            app.UseMiddleware<EnsureResponseNotStartedMiddleware>();
             
        }
    }

    public class EnsureResponseNotStartedMiddleware
    {
        private readonly RequestDelegate _next;

        public EnsureResponseNotStartedMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task InvokeAsync(HttpContext context)
        {
            if (!context.Response.HasStarted)
            {
                return _next(context);
            }

            return Task.CompletedTask;
        }
    }
}

