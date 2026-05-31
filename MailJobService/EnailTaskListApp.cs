using MailEnhanceService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;  
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq; 
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading;  
using System.Threading.Tasks; 

namespace MailTaskJobService
{
    /// <summary>
    /// Email 任務單元 以一個 任務為單位，包含從獲取郵件模板內容、（解析模板）、發送郵件等整個流程
    /// </summary>
    public class EnailTaskListApp
    {
        private ILogger<EnailTaskListApp> _logger;
        private string _languageCode; 
        private string _mailDataApiUrl;
        private IPubBusiness _pubBusiness;
        private readonly IList<AuthenticUserModel> _authenticUserModelList;
        private readonly string _mailToDefault;
        private readonly int _processMinutes = 1;
        private readonly IMailTaskServiceSetting _mailTaskServiceSetting;
        
        public EnailTaskListApp(IOptions<string> MailDataApiUrl, IMailTaskServiceSetting mailTaskServiceSetting, IOptions<string> languageCode, IPubBusiness pubBusiness, ILogger<EnailTaskListApp> logger)
        { 
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
             
            _languageCode = languageCode.Value;
            _pubBusiness = pubBusiness ?? throw new ArgumentNullException(nameof(pubBusiness));
            _authenticUserModelList = _pubBusiness.GetAuthenticUsers();

            _mailTaskServiceSetting = mailTaskServiceSetting;
            _mailDataApiUrl = mailTaskServiceSetting.MailDataApiUrl;
            _processMinutes = _mailTaskServiceSetting.IntervalMinutes > 0 ? _mailTaskServiceSetting.IntervalMinutes : 1; // 默認1分鐘
            _mailToDefault = _mailTaskServiceSetting.MailToDefault ?? string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailList"></param>
        /// <param name="mainComId"></param>
        /// <param name="shopId"></param>
        /// <param name="subject"></param>
        /// <param name="mailTemplateEnum"> 由於來自 IshopX ， 這裡的 Email 內容已經是拼裝了的，所以 mailTemplateEnum = NO_TEMPLATE </param>
        /// <param name="LanguageCode"></param>
        /// <param name="callBackUrl"></param>
        /// <param name="attachPath"></param>
        /// <returns>List of SendingResult</returns>
        public async Task<ResponseModalX> Run(string[] mailToList, AuthenticUserModel emailModel, string subject, string emailContent,
            string callBackUrl, string languageCode, MailTemplateEnum mailTemplateEnum = MailTemplateEnum.NO_TEMPLATE)
        {
            //Console.Clear();
            Console.OutputEncoding = Encoding.UTF8; 
             
            #region BEGIN
            //==============================================================================================================================
               
            Stopwatch sw = new Stopwatch();
            sw.Start();

            _logger.LogInformation("{mainComId} MailService/SendMail called at {Time}", emailModel.MainComId, DateTime.Now);

            // 初始化响应对象
            var response = new ResponseModalX
            {
                meta = new MetaModalX
                {
                    Success = false,
                    Message = "郵件發送失敗",
                    ErrorCode = 1
                },
                data = new { }
            };

            try
            {
                // 1. 基础参数验证
                if (emailModel == null)
                {
                    response.meta.Message = "請提供有效的用戶認證參數(appsettings.json)";
                    return response;
                }

                if (string.IsNullOrEmpty(emailModel.MainComId))
                {
                    response.meta.Message = "缺少必要參數：mainComId";
                    return response;
                } 

                if (string.IsNullOrEmpty(_languageCode))
                    _languageCode = "zh-HK";
  
                // 默认测试邮箱 
                if (mailToList.Count() == 0)
                    mailToList = new[] { _mailToDefault }; 
                 
                // 6. 初始化邮件服务并发送

                var emailAppService = EmailAppService.StartUpEmailAppService(emailModel);
                _logger.LogInformation("🌅 [2026-05-17 10:36:50 480]🌠 [INFO] [StartUp EmailAppService.StartUpEmailAppService] [{MainComId}]", emailModel.MainComId);
                _logger.LogInformation("EmailAppService started for MailService/SendMail.");

                List<SendingResult> sendingResult = await emailAppService.RunWithResultsAsync(
                    mailToList,
                    subject,
                    mailTemplateEnum,
                    emailContent,
                    _languageCode,
                    callBackUrl ?? null,
                    null  // 任務Email 不接入本地文件，所有內容都是來自第三方，例如IshopX 
                );
                int successCount = sendingResult.Count(r => r.Result);
                int failCount = sendingResult.Count(r => !r.Result);
                int totalCount = sendingResult.Count==0?1: sendingResult.Count;
                // 7. 处理发送结果 過半發送成功即視為整體成功，並且記錄日誌以便後續檢查
                if (successCount / totalCount > 0.5)
                {
                    response.meta.Success = true;
                    response.meta.Message = $"The email has been sent successfully. Total={totalCount}, Success={successCount}, Fail={failCount}";
                    response.meta.ErrorCode = 0;
                }
                else
                {
                    response.meta.Message = $"Email failed to send, please check the log. ,Total={totalCount}, Success={successCount}, Fail={failCount}";
                }
                response.data = sendingResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MailService/SendMail An error occurred while sending email.");
                response.meta.Message = $"Internal error：{ex.Message}";
                response.meta.ErrorCode = 500;
            }
              
            string time = sw.Elapsed + "-(" + sw.Elapsed.Seconds + " seconds)";
            sw.Stop();
             
            _logger.LogInformation(">>> Elapsed Time = {0}", sw.Elapsed);

            //await Task.CompletedTask; 
            #endregion END 

            return response;
        }
         
        /// <summary>
        /// 请求 Ishop 平台获取当前待发送的邮件任务
        /// API: /Mgr/ShopAdmin/GetMailTaskJobRequest (POST)
        /// 需要携带 Bearer Token 认证
        /// </summary>
        /// <returns>包装了邮件任务数据的响应对象</returns>
      
        /// <summary>
        /// 请求 Ishop 平台获取当前待发送的邮件任务
        /// API: POST /mgr/ShopMailService/GetMailTaskJobRequest
        /// 请求体: shopId=sh0006
        /// </summary>
        /// <param name="authenticUser">认证信息（包含 Bearer Token）</param>
        /// <param name="shopId">店铺ID（若传入非空则使用，否则回退到 authenticUser.ShopId）</param>
        private async Task<ResponseModalX> TaskList(AuthenticUserModel authenticUser)
        {
            var response = new ResponseModalX
            {
                meta = new MetaModalX
                {
                    Success = false,
                    Message = "Retrieve Email Task List Fail",
                    ErrorCode = 1
                },
                data = null
            };

            // 1. 参数验证
            if (authenticUser == null)
            {
                _logger.LogError("[INFO][Func::TaskList] No AuthenticUserModel found. Please check authentication configuration.");
                response.meta.Message = "未找到任何有效的认证配置";
                return response;
            }

            if (string.IsNullOrEmpty(authenticUser.ShopId))
            {
                _logger.LogError("ShopId is missing. Please set up the AuthenticUser of appsettings.json.");
                response.meta.Message = "缺少店铺ID参数 (shopId)";
                return response;
            }

            // 2. 确定 shopId（优先使用传入的，否则使用 authenticUser 中的）
            string effectiveShopId = authenticUser.ShopId;
             
            // 3. 构建完整 API 地址（根据实际路由调整）
            string apiUrl = $"{_mailDataApiUrl.TrimEnd('/')}/mgr/ShopMailService/GetMailTaskJobRequest";
            string logMessage = $"\n🌄 [[INFO][Func::TaskList] [EMAIL DATA SOURCE API URL] 🎡 [Requesting MailTask from API: 🌇\n🌄 [{apiUrl}] [ShopId: {effectiveShopId}] 🌄\n";
            _logger.LogInformation(logMessage);
            await Console.Out.WriteAsync(logMessage);

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(60);

            // 4. 添加 Authorization Header
            string token = authenticUser.AuthenticationCode;
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            try
            {
                // 5. 准备 POST 表单数据（x-www-form-urlencoded）
                var formData = new Dictionary<string, string>
                {
                    { "shopId", effectiveShopId }
                };

                var content = new FormUrlEncodedContent(formData);
                 
                // 6. 发送 POST 请求
                var httpResponse = await httpClient.PostAsync(apiUrl, content);
                string responseBody = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    // 7. 反序列化返回的 MailTaskJobRequest
                    var tasklistRes = JsonConvert.DeserializeObject<MailTaskJobRequest>(responseBody);
                    if (tasklistRes != null && tasklistRes.Success)
                    {
                        // 如果 appsettings.json 中 MailTaskServiceSetting 配置的 MailTaskServiceSetting.TestMode  0=測試模式，1=正常模式
                        // （mailto 目標EMAIL地址列表為默認地址 [MailToDefault]）
                        if(_mailTaskServiceSetting.TestMode == (int)ServiceSettingMode.TEST_MODE)
                        {
                            tasklistRes.ToMailList = new List<EmailAndName>
                            {
                                new EmailAndName
                                {
                                    Enail = _mailToDefault,
                                    Name = "Test Mode Default Recipient"
                                }
                            };
                        }

                        response.meta.Success = true;
                        response.meta.Message = "Email retrieval task successful";
                        response.meta.ErrorCode = 0;
                        response.data = tasklistRes;

                        string tasklisTips = $"\n🌄 [[INFO][Func::TaskList] [EMAIL DATA SOURCE API URL] 🎡 [Successfully retrieved mail task. TaskId: {tasklistRes.EmailTaskId}, Recipients: {tasklistRes.ToMailList?.Count ?? 0}] 🌇\n";
                        _logger.LogInformation(tasklisTips);
                        await Console.Out.WriteAsync(tasklisTips);
                    }
                    else
                    {
                        response.meta.Message = tasklistRes?.Success == false
                            ? "API GetMailTaskJobRequest Return failed: " + (tasklistRes?.Subject ?? "No detailed information")
                            : "The task object obtained through deserialization is invalid.";
                        _logger.LogWarning(response.meta.Message, "\n" ,responseBody);

                        string tasklisTips = $"\n🌄 [[INFO][Func::TaskList] [EMAIL DATA SOURCE API URL] 🎡 [{response.meta.Message} 🌇 responseBody:\n {responseBody}]\n"; 
                        await Console.Out.WriteAsync(tasklisTips);
                    }
                }
                else
                {
                    response.meta.Message = $"HTTP request failed, status code: {httpResponse.StatusCode}，response: {responseBody}";
                    _logger.LogError(response.meta.Message);
                }
            }
            catch (TaskCanceledException tex)
            {
                response.meta.Message = "Request timed out";
                _logger.LogError(tex, "Email task API request timed out");
            }
            catch (Exception ex)
            {
                response.meta.Message = $"Request exception: {ex.Message}";
                _logger.LogError(ex, "An exception occurred while calling GetMailTaskJobRequest.");
            }

            return response;
        }

        // 获取邮件模板内容
        // mainComId > shopId > default template
        public string GetMailTemplate(string mainComId, string shopId, MailTemplateEnum mailTemplateEnum,string LanguageCode)
        {
            string content = "  ";
            string fileName = string.Format("{0}_{1}.html", mailTemplateEnum.ToString(), LanguageCode);
            string pathFileTemplate = Path.Combine(Directory.GetCurrentDirectory(), "MailTemplate", fileName);
            if (!string.IsNullOrEmpty(mainComId))
            {
                string pathFileTemplateForMainComId = Path.Combine(Directory.GetCurrentDirectory(), "MailTemplate", mainComId, fileName);
                if (File.Exists(pathFileTemplateForMainComId))
                {
                    pathFileTemplate = pathFileTemplateForMainComId;
                }
            }

            // shopId level template has higher priority than mainComId level template
            if (!string.IsNullOrEmpty(shopId))
            {
                string shopFileName = $"{shopId}_{fileName}";
                string pathFileTemplateForShopIdOfMainComId = Path.Combine(Directory.GetCurrentDirectory(), "MailTemplate", mainComId, shopFileName);
                if (File.Exists(pathFileTemplateForShopIdOfMainComId))
                {
                    pathFileTemplate = pathFileTemplateForShopIdOfMainComId;
                }
            }

            if (!File.Exists(pathFileTemplate))
            {
                _logger.LogInformation("MAIL TEMPLATE PATH IS NOT EXIST [{0}]", pathFileTemplate); 
                return content;
            }
            try
            {
                content = File.ReadAllText(pathFileTemplate, Encoding.UTF8);

                return content;
            }
            catch
            { 
                return content;
            }
        }

        /// <summary>
        /// 批處理 店鋪列表任務
        /// 🟥🟥🟥🟥🟥🟥🟥🟥🟥🟥
        /// </summary>
        /// <param name="emailModel"></param>
        /// <returns></returns>
        public async Task BatchListRun()
        {
            foreach (var emailModelitem in _pubBusiness.GetAuthenticUsers())
            {
                if (emailModelitem == null || string.IsNullOrEmpty(emailModelitem.ShopId))
                {
                    string invalidAuthenticUserModelTips = $"\n🌄 [IMPORTANT] [MAIL TASK SERVICE] 🎡 [Skipping Invalid AuthenticUserModel With Missing ShopId{emailModelitem.ShopId??"null"} Config.] 🌇\n";
                    Console.WriteLine(invalidAuthenticUserModelTips);
                    _logger.LogWarning(invalidAuthenticUserModelTips);

                    string noConfigAspSettingHints = $"\n🌄 [IMPORTANT] [NO CONFIG ASPSETTING HINTS] 🎡 [Skipping Invalid AuthenticUserModel With Missing ShopId{emailModelitem.ShopId??"null"} Config.] 🌇\n";
                    Console.WriteLine(noConfigAspSettingHints);
                    _logger.LogWarning(noConfigAspSettingHints);

                    continue;
                }
                // 1. 请求 Ishop 平台获取当前待发送的邮件任务   
                ResponseModalX response = await TaskList(emailModelitem);
                if (response.meta.Success)
                {
                    MailTaskJobRequest taskJob = (MailTaskJobRequest)response.data;
                    string[] mailToList = taskJob.ToMailList?.Select(x => x.Enail).ToArray() ?? new string[0];
                    string emailContent = Base64Helper.TryDecodeBase64(taskJob.EmailBody);

                    // 2.成功获取到邮件任务，开始处理前，標記任務為佔用中，避免其他實例重複獲取同一任務
                    var responseOccupied = await UpdateTaskOccupied(emailModelitem, taskJob.EmailTaskId);
                    if (!responseOccupied.meta.Success)
                    {
                        _logger.LogError($"Exec UpdateTaskOccupied Fail: {responseOccupied.meta.Message}");
                        continue;
                    }
#if DEBUG
                    await Console.Out.WriteLineAsync($"🌅 [2026-05-17 10:36:50 480]🌠 [TESTING] [BatchListRun] [{string.Join(",", mailToList)}]");
#endif

                    // 群發郵件任務執行
                    ResponseModalX responseModalX = await Run(mailToList, emailModelitem, taskJob.Subject, emailContent,
                            taskJob.CallBackUrl, _languageCode);

                    // 3.執行Email任務後的回調處理，例如通知Ishop平台該任務已完成
                    // 通過接口 http://localhost:59136/mgr/ShopMailService/UpdateEmailTaskJobComplete
                    if (responseModalX.meta.Success)
                    {
                        string someEmailfailed = $"\nIf multiple mailing lists are sent, the result may be a failure, which only indicates that the emails to individual addresses failed to be delivered.\n"; ;
                        string msg = $"\n🌅 Successfully processed mail task for ShopId: {emailModelitem.ShopId}, TaskId: {taskJob.EmailTaskId} 🌅 \n❎ 🌠  {someEmailfailed} 🌠 \n";
                        //------------------------------------------------------------
                        _logger.LogInformation(msg);
                        await Console.Out.WriteLineAsync(msg);
                        //------------------------------------------------------------
                        _logger.LogInformation(responseModalX.meta.Message);
                        await Console.Out.WriteLineAsync(responseModalX.meta.Message);

                        // 调用 Ishop 平台更新任务状态
                        bool updated = await UpdateTaskComplete(emailModelitem, taskJob.EmailTaskId);
                        if (updated)
                        {
                            _logger.LogInformation("Task {EmailTaskId} marked as completed on Ishop platform.", taskJob.EmailTaskId);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to mark task {EmailTaskId} as completed, but email was sent. Manual check may be needed.", taskJob.EmailTaskId);
                        }

                        // 處理那些郵件成功發送，那些失敗 responseModalX.data 是 List<SendingResult>，裡面有每個郵件的發送結果，可以進行更細粒度的回調處理，例如對失敗的郵件進行重試或者記錄到數據庫等
                        // 获取发送结果列表
                        if (responseModalX.data is List<SendingResult> sendingResults && sendingResults.Any())
                        {
                            bool reported = await ReportSendingResultsAsync(emailModelitem, sendingResults);
                            if (!reported)
                            {
                                _logger.LogWarning("Failed to report sending results for task {TaskId}, but emails have been processed.", taskJob.EmailTaskId);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to process mail task for ShopId: {ShopId}, TaskId: {TaskId}. Error: {ErrorMessage}",
                            emailModelitem.ShopId, taskJob.EmailTaskId, responseModalX.meta.Message);
                    }

                    // 隨機停止 波動範圍 50-60%
                    int intervalSleepSec = IntervalRadomSecond(_processMinutes, mailToList.Count());
                    Thread.Sleep(intervalSleepSec*1000); // millseconds
                }
                else
                {
                    _logger.LogInformation($"[TaskList][Response.Meta.Success={response.meta.Success}] No pending mail task for ShopId: {emailModelitem.ShopId}. Message: {response.meta.Message}");
                }
            }
        }

        /// <summary>
        /// 通知 Ishop 平台，指定邮件任务已完成。
        /// </summary>
        /// <param name="authenticUser">认证信息（含 ShopId 和 Token）</param>
        /// <param name="emailTaskId">任务 ID</param>
        /// <returns>是否更新成功</returns>
        private async Task<bool> UpdateTaskComplete(AuthenticUserModel authenticUser, string emailTaskId)
        {
            if (authenticUser == null || string.IsNullOrEmpty(authenticUser.ShopId))
            {
                _logger.LogError("Invalid AuthenticUserModel or missing ShopId.");
                return false;
            }

            string apiUrl = $"{_mailDataApiUrl.TrimEnd('/')}/mgr/ShopMailService/UpdateEmailTaskJobComplete";
            _logger.LogInformation("Calling Update API: {ApiUrl} for TaskId: {TaskId}", apiUrl, emailTaskId);

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authenticUser.AuthenticationCode}");

            try
            {
                // 准备 POST 表单数据（emailTaskId 作为表单字段）
                var formData = new Dictionary<string, string>
                {
                    { "shopId", authenticUser.ShopId},
                    { "emailTaskId", emailTaskId }
                };
                var content = new FormUrlEncodedContent(formData);

                var response = await httpClient.PostAsync(apiUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // 尝试解析响应中的 Success 字段
                    var result = JsonConvert.DeserializeObject<ResponseModalX>(responseBody);
                    if (result != null && result.meta.Success)
                    {
                        _logger.LogInformation("Task {TaskId} successfully updated on Ishop.", emailTaskId);
                        return true;
                    }
                    else
                    {
                        _logger.LogError("Update API returned failure. Response: {Response}", responseBody);
                        return false;
                    }
                }
                else
                {
                    _logger.LogError("HTTP error calling Update API. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, responseBody);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while calling UpdateTaskComplete for TaskId {TaskId}", emailTaskId);
                return false;
            }
        }
         
        /// <summary>
        /// 通知 Ishop 平台，指定邮件任务正在獨佔中。
        /// </summary>
        /// <param name="authenticUser">认证信息（含 ShopId 和 Token）</param>
        /// <param name="emailTaskId">任务 ID</param>
        /// <returns>是否更新成功</returns>
        private async Task<ResponseModalX> UpdateTaskOccupied(AuthenticUserModel authenticUser, string emailTaskId)
        {
            var response = new ResponseModalX
            {
                meta = new MetaModalX
                {
                    Success = false,
                    Message = "Fail to exec UpdateTaskOccupied",
                    ErrorCode = 1
                },
                data = null
            };

            if (authenticUser == null || string.IsNullOrEmpty(authenticUser.ShopId))
            {
                _logger.LogError("Invalid AuthenticUserModel or missing ShopId.");
                response.meta.Message = "Invalid AuthenticUserModel or missing ShopId.";
                response.meta.ErrorCode = 1;
                return response;
            }

            string apiUrl = $"{_mailDataApiUrl.TrimEnd('/')}/mgr/ShopMailService/UpdateTaskOccupied";
            _logger.LogInformation("Calling Update API: {ApiUrl} for TaskId: {TaskId}", apiUrl, emailTaskId);

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authenticUser.AuthenticationCode}");

            try
            {
                // 准备 POST 表单数据（shopId,emailTaskId 作为表单字段）
                var formData = new Dictionary<string, string>
                {
                    { "shopId", authenticUser.ShopId},
                    { "emailTaskId", emailTaskId }
                };
                var content = new FormUrlEncodedContent(formData);

                var responseMessage = await httpClient.PostAsync(apiUrl, content);
                string responseBody = await responseMessage.Content.ReadAsStringAsync();

                if (responseMessage.IsSuccessStatusCode)
                {
                    // 尝试解析响应中的 Success 字段
                    var result = JsonConvert.DeserializeObject<ResponseModalX>(responseBody);
                    if (result != null && result.meta.Success)
                    {
                        _logger.LogInformation("Update EmailTask Occupied {TaskId} successfully updated to occupy on Ishop.", emailTaskId);
                        return result;
                    }
                    else
                    {
                        _logger.LogError("Update EmailTask Occupied returned failure. Response: {Response}", MailCommonBase.GetHtmlText(responseBody));
                        return result;
                    }
                }
                else
                {
                    _logger.LogError("HTTP error calling Update UpdateTaskOccupied. Status: {StatusCode}, Response: {Response}",
                        responseMessage.StatusCode, MailCommonBase.GetHtmlText(responseBody));
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while calling UpdateTaskOccupied for TaskId {TaskId}", emailTaskId);
                response.meta.Message = $"Exception while calling UpdateTaskOccupied: {ex.Message}";
                return response;
            }
        }

        // <param name="emailsTotal">邮件总数量</param>
        /// <returns>随机休眠秒数（整数）</returns>
        public int IntervalRadomSecond(int ProcessMinutes, long emailsTotal)
        {
            // 1. 边界防护：避免除零、负数
            if (ProcessMinutes <= 0 || emailsTotal <= 0)
            {
                return 1; // 默认1秒，防止异常
            }

            // 2. 计算基础间隔：总秒数 ÷ 邮件总数（理论平均间隔）
            int totalSeconds = ProcessMinutes * 60;
            double baseInterval = (double)totalSeconds / emailsTotal;

            // 3. 生成随机波动值：±10%~20% 浮动，无规律
            Random random = new Random(Guid.NewGuid().GetHashCode());

            // 大波动：-55% ~ +55%（50%-60%范围）
            double fluctuation = random.NextDouble() * 1.1 - 0.55;

            // 4. 计算最终随机间隔（最小不低于1秒，防止过快）
            int randomInterval = (int)(baseInterval * (1 + fluctuation));
            return Math.Max(randomInterval, 1);
        }

        /// <summary>
        /// 回传邮件发送结果到 IshopX 平台，用于更新每个收件人的发送失败计数
        /// </summary>
        /// <param name="authenticUser">认证信息（ShopId、Token 等）</param>
        /// <param name="sendingResults">发送结果列表</param>
        /// <returns>是否成功通知平台</returns>
        private async Task<bool> ReportSendingResultsAsync(AuthenticUserModel authenticUser, List<SendingResult> sendingResults)
        {
            if (authenticUser == null || string.IsNullOrEmpty(authenticUser.ShopId))
            {
                _logger.LogError("Invalid AuthenticUserModel or missing ShopId for result callback.");
                return false;
            }

            if (sendingResults == null || sendingResults.Count == 0)
            {
                _logger.LogWarning("No sending results to report for ShopId: {ShopId}", authenticUser.ShopId);
                return true; // 没有结果可上报，视为成功
            }

            // 1. 构建 API 地址
            string apiUrl = $"{_mailDataApiUrl.TrimEnd('/')}/mgr/ShopMailService/BatchCalcEmailSubscribeSentFail";
             
            // 2. 构建请求体（与 IshopX 控制器中的 BatchCalcSentFailRequest 一致）
            var requestBody = new
            {
                emailModel = new
                {
                    MainComId = authenticUser.MainComId,
                    ShopId = authenticUser.ShopId,
                    UserName = authenticUser.UserName,
                    AuthenticationCode = authenticUser.AuthenticationCode,
                    AuthenticMethods = authenticUser.AuthenticMethods ?? "Bearer"
                },
                sendingResultList = sendingResults.Select(r => new
                {
                    RecipientEmail = r.RecipientEmail,
                    Result = r.Result
                }).ToList()
            };

            string jsonPayload = JsonConvert.SerializeObject(requestBody);
            _logger.LogInformation("Reporting sending results to IshopX: {ApiUrl}, Payload: {Payload}", apiUrl, jsonPayload);

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authenticUser.AuthenticationCode}");

            try
            {
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<ResponseModalX>(responseBody);
                    if (result != null && result.meta.Success)
                    {
                        _logger.LogInformation("Successfully reported {Count} sending results for ShopId: {ShopId}",
                            sendingResults.Count, authenticUser.ShopId);
                        return true;
                    }
                    else
                    {
                        _logger.LogError("IshopX API returned failure: {Message}, Response: {Response}",
                            result?.meta.Message, responseBody);
                        return false;
                    }
                }
                else
                {
                    _logger.LogError("HTTP error when reporting results. Status: {StatusCode}, Response: {Response}",
                        response.StatusCode, responseBody);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while reporting sending results for ShopId: {ShopId}", authenticUser.ShopId);
                return false;
            }
        }
    }
}
