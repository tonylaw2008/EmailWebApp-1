using MailEnhanceService;   
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection; 
using Microsoft.Extensions.Hosting; 
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz.Impl.Triggers;
using System; 
using System.Collections.Generic;
using System.Configuration;
using System.IO; 
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks; 
using static MailTaskJobService.MailTaskServiceQuartz;

namespace MailTaskJobService
{
    class Program
    {
        static void Main(string[] args)
        {  
            //==============================================================================================================================
            //Console.Clear();
            Console.OutputEncoding = Encoding.UTF8;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            string languageCode = "zh-HK"; // 默認語言代碼
            if(args.Length > 0)
            {
                languageCode = args[0].Trim(); // 從命令行參數獲取語言代碼
            }
              
            // 创建ServiceCollection
            var services = new ServiceCollection();
            MailTaskServiceQuartz.ConfigureServices(services, languageCode);

            // 创建ServiceProvider
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            var mailTaskServiceSetting = serviceProvider.GetRequiredService<IMailTaskServiceSetting>();
            var Configuration = serviceProvider.GetRequiredService<IConfiguration>();

            // MailDataApiUrl  EMAIL ADDRESS 資源來源 
            string msgMailSourceTips = $"\n🌄 [IMPORTANT][EMAIL DATA SOURCE API URL] 🎡 [{mailTaskServiceSetting.MailDataApiUrl}] 🌇\n";
            logger.LogInformation(msgMailSourceTips);
            Console.WriteLine(msgMailSourceTips);
            Console.WriteLine($"\n🌄 🎡 🌇 loading.....\n");

            // 顯示 是否處於測試模式 TestMode
            if(mailTaskServiceSetting.TestMode == (int)ServiceSettingMode.TEST_MODE)
            {
                string texModeTips = $"\n🌌𖣯𖠁 [IMPORTANT][ON TEST MODE] 🎡 [{mailTaskServiceSetting.MailToDefault}] [TEST_MODE]🌇🌌\n";
                logger.LogInformation(texModeTips);
                Console.WriteLine(texModeTips); 
            }


            Thread.Sleep(16000);
            var intervalMinutes = mailTaskServiceSetting.IntervalMinutes;
            logger.LogInformation($"\n🟩[Start Mail Task Job Service] \n🎡 [Circle IntervalMinutes = {intervalMinutes}]\n🎡 [Circle MailToDefault = {intervalMinutes}]\n");

            try
            { 
                logger.LogInformation("\n🟩 OK.............................................................................................\n");
                Console.WriteLine("\n🟩 OK.............................................................................................\n");
                // 啟動 Schedule 
                var mailTaskServiceQuartz = serviceProvider.GetRequiredService<MailTaskServiceQuartz>();

                // 启动调度，假设立即开始，间隔n分钟
                mailTaskServiceQuartz.RunGlobalProgram(DateTime.Now).GetAwaiter();

                logger.LogInformation("\n🟩 Quartz scheduler started.\n");
                Console.WriteLine("\n🟩 Quartz scheduler started.\n");
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex, "[APP RUNNING EXCEPTION ERROR OCCURED ]");
                MailCommonBase.OperateDateLoger("[APP RUNNING EXCEPTION ERROR OCCURED ]" + ex.Message);
            }

            Console.WriteLine("\n\n🟩 Press Esc to exit while the program is running...\n\n");

            Task taskCircleInput = new Task(() =>
            {
                while (true)
                { 
                    // 偵測 Esc 鍵
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true).Key;
                        if (key == ConsoleKey.Escape)
                        {
                            Console.WriteLine("\n\nEsc key detected, exiting...");
                            Console.WriteLine("\n\nThe program will be exited safely.。");
                            Environment.Exit(0);
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"\n\nKey {key} detected, but only Esc will exit.");
                        }
                    }

                    // 休眠一段時間，避免 CPU 100%
                    Thread.Sleep(1000);
                }

            });

            taskCircleInput.Start();
            taskCircleInput.Wait();

            

            Console.ReadKey();
            Console.ReadKey();
            Console.ReadKey();
            Environment.Exit(0);


        }
    } 
}
