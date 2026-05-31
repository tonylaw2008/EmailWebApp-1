using MailEnhanceService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers; 
using static Quartz.MisfireInstruction;

namespace MailTaskJobService
{
    public class MailTaskServiceSetting: IMailTaskServiceSetting
    {
        public string MailDataApiUrl { get; set; }
        public int IntervalMinutes { get; set; }
        public string MailToDefault { get; set; }
        public int TestMode { get; set; }
    }

    public interface IMailTaskServiceSetting
    {
        string MailDataApiUrl { get; set; }
        int IntervalMinutes { get; set; }
        string MailToDefault { get; set; }
        int TestMode { get; set; }
    }

    public partial class MailTaskServiceQuartz
    {
        private int _intervalMinutes;
        private string _languageCode;
        public MailTaskServiceQuartz(IConfiguration configuration)
        {
            _intervalMinutes = configuration.GetValue<int>("intervalMinutes");
            _languageCode = "zh-HK"; // 默認語言代碼
        }

        public async Task RunGlobalProgram(DateTime TaskMonthlyStartDate)
        {
            if (_intervalMinutes == 0)
            {
                _intervalMinutes = 1;
            }
           
            try
            {
                NameValueCollection props = new NameValueCollection
                {
                    { "quartz.serializer.type", "binary" }
                };
                StdSchedulerFactory factory = new StdSchedulerFactory(props);
                IScheduler scheduler = await factory.GetScheduler();

                await scheduler.Start();

                IJobDetail job = JobBuilder.Create<MailTaskServiceJOB>()
                    .WithIdentity("MailTaskServiceJOB", "GROUP1")
                    .Build();
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity("MailJobServiceTRIGER")
                    .StartAt(TaskMonthlyStartDate)
                    .ForJob("MailTaskServiceJOB", "GROUP1")
                    .WithCalendarIntervalSchedule(w => w
                    .WithIntervalInMinutes(_intervalMinutes)
                    )  
                    .Build();
                 
                await scheduler.ScheduleJob(job, trigger);
                 
                await Task.Delay(TimeSpan.FromMilliseconds(1000)); 
            }
            catch (SchedulerException se)
            {
                await Console.Error.WriteLineAsync(se.ToString());
            }
        }
        
       public class MailTaskServiceJOB : IJob
        { 
            public async Task Execute(IJobExecutionContext context)
            {
                // 1. 先创建 ServiceCollection 并配置
                var services = new ServiceCollection();
                string languageCode = "zh-HK"; // 或者从 appsettings 读取
                MailTaskServiceQuartz.ConfigureServices(services, languageCode);

                // 2. 再构建 ServiceProvider
                var serviceProvider = services.BuildServiceProvider();
                var logger = serviceProvider.GetRequiredService<ILogger<MailTaskServiceJOB>>();
                var pubBusiness = serviceProvider.GetRequiredService<IPubBusiness>();
                pubBusiness.ClearConsole(); // 清除控制台日志 按定時規則執行時，避免屏幕日誌過多影響閱讀

                try
                {
                    var enailTaskListApp = serviceProvider.GetRequiredService<EnailTaskListApp>();
                    logger.LogInformation("BatchListRun started...");
                    await enailTaskListApp.BatchListRun();
                    logger.LogInformation("BatchListRun completed.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Execute exception");
                    MailCommonBase.OperateDateLoger("[Execute Exception] " + ex.Message);
                }

                await Console.Out.WriteLineAsync($"\n🌅 [{DateTime.Now:yyyy-MM-dd HH:mm:ss fff}]🌠 [INFO] [Execute::MailJobServiceJOB:{context.FireInstanceId}]\n");
            }
        }

        public static IConfigurationRoot ReadFromAppSettings()
        {
            return new ConfigurationBuilder()
                         .SetBasePath(Directory.GetCurrentDirectory())
                         .AddJsonFile("appsettings.json", false)
                         .AddEnvironmentVariables()
                         .Build();
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public static void ConfigureServices(IServiceCollection services, string languageCode)
        {
            // 创建 appsettings
            var Configuration = ReadFromAppSettings();

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IConfigurationRoot>(Configuration);

            var _languageCode = Configuration.GetSection("languageCode").Get<string>()?? languageCode;
            services.AddSingleton(Options.Create(_languageCode));

            // 提供公司用戶認證服務列表 
            var authenticUserList = Configuration.GetSection("AuthenticUser").Get<List<AuthenticUserModel>>();
            PubBusiness pubBusiness = new PubBusiness(authenticUserList);
            services.AddSingleton<IPubBusiness>(pubBusiness);   // ✅ 直接使用已创建的实例 

            // 默認發送目標郵箱地址  EnailTaskListApp
            var mailToDefault = Configuration.GetSection("MailToDefault").Get<string>() ?? string.Empty;
            services.AddSingleton(Options.Create(mailToDefault));

            // 注册 EnailTaskListApp
            services.AddTransient<EnailTaskListApp>();

            // MailDataApiUrl
            var mailDataApiUrl = Configuration.GetSection("MailDataApiUrl").Get<string>() ?? string.Empty;
            services.AddSingleton(Options.Create(mailDataApiUrl));

            // ✅ 注册 intervalMinutes（值类型单例）  
            var mailTaskServiceSetting = Configuration.GetSection("MailTaskServiceSetting").Get<MailTaskServiceSetting>();
            services.AddSingleton<IMailTaskServiceSetting>(mailTaskServiceSetting);
             
            // Add Scedule App Service
            services.AddTransient<MailTaskServiceQuartz>();

            // 配置日志
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddLog4Net("log4net.config"); 
            });
        }
    } 
}


