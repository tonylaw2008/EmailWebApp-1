using log4net;
using MailEnhanceService;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MailEnhanceService
{
    public class PubBusiness:IPubBusiness
    {
        private readonly IList<AuthenticUserModel> authenticUserModelList;
        public PubBusiness(IList<AuthenticUserModel> _authenticUserModelList)
        {
            authenticUserModelList = _authenticUserModelList;
        }

        // 授權認證方法，驗證 mainComId 和 authHeader 是否有效
        public bool IsValidAuth(string mainComId, string authHeader)
        {
            // 1. 參數非空驗證
            if (string.IsNullOrEmpty(mainComId) || string.IsNullOrEmpty(authHeader))
                return false;

            // 2. 根據 mainComId 尋找對應的認證用戶
            var user = authenticUserModelList?.FirstOrDefault(u => u.MainComId == mainComId);
            if (user == null)
                return false;

            // 3. 取得期望的認證方法（如 "Bearer"），預設為 "Bearer"
            string expectedMethod = user.AuthenticMethods ?? "Bearer";

            // 4. 檢查 authHeader 是否以期望的方法開頭（不區分大小寫）
            if (!authHeader.StartsWith(expectedMethod, StringComparison.OrdinalIgnoreCase))
                return false;

            // 5. 提取 token 部分（去掉方法前綴和空格）
            string token = authHeader.Substring(expectedMethod.Length).Trim();

            // 6. 與儲存的 AuthenticationCode 比較  必須 加上 前綴Bearer
            return token == user.AuthenticationCode;
        }

        public IList<AuthenticUserModel> GetAuthenticUsers()
        {
            return authenticUserModelList; 
        }

        // 清理控制台畫面的工具函數
        public void ClearConsole()
        {
            // 複製上面的 ClearConsole 邏輯
            try
            {
                if (OperatingSystem.IsWindows())
                {
                    Console.Clear();

                    string msgApp = $"\n🌄 [MAIL TASK JOB SERVICE] 🎡 🌇\n";
                    Console.WriteLine(msgApp);

                    Console.WriteLine("\n🟩 Quartz scheduler started.\n");
                   
                }
                else
                {
                    Console.Write("\u001b[2J\u001b[H");
                    Console.Out.Flush();

                    string msgApp = $"\n🌄 [MAIL TASK JOB SERVICE] 🎡 🌇\n";
                    Console.WriteLine(msgApp);

                    Console.WriteLine("\n🟩 Quartz scheduler started.\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Clear console error: {ex.Message}");
            }
        }
    }
     
    public interface IPubBusiness
    {
        bool IsValidAuth(string mainComId, string authHeader);
        public IList<AuthenticUserModel> GetAuthenticUsers();

        void ClearConsole();
    }

    // EmailAndName 對象
    public class EmailAndName : IEmailAndName
    {
        [EmailAddress]
        public string Enail { get; set; }

        [DefaultValue("")]
        public string Name { get; set; } 
    } 

    public interface IEmailAndName
    {
        public string Enail { get; set; }
        public string Name { get; set; }
    }

    public class MailTaskJobRequest
    {
        public bool Success { get; set; }
        // 發送前，委派任務Id，以提供請求，
        // 但數據庫中，這裡的EmailTaskId可以是以逗號間隔的多個個任務ID，
        public string EmailTaskId { get; set; }
        public string ShopId { get; set; }
        public string CallBackUrl { get; set; }
        public List<EmailAndName> ToMailList { get; set; }
        public string Subject { get; set; }
        public string EmailBody { get; set; }
    }

    // 發送結果對象
    public class SendingResult
    {
        [EmailAddress]
        [Required]
        public string RecipientEmail { get; set; }

        [DefaultValue(true)]
        public bool Result { get; set; }
    }
}
