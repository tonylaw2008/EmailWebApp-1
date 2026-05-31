using EamilWebApp.Models;
using MailEnhanceService;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using System.Text.Unicode;

namespace EamilWebApp.Controllers
{
    [Route("MailService")]
    [ApiController]
    public class MailServiceController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IList<AuthenticUserModel> _authenticUserModelList;
        private readonly string _mailToDefault;
        private string _httpHost { get; set; }
       
        // 公司用戶認證服務接口，通過依賴注入獲取實現類的實例
        private readonly IPubBusiness _pubBusiness;
        public MailServiceController(
            ILogger<HomeController> logger, 
            IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor context, 
            IList<AuthenticUserModel> authenticUserModelList, 
            IPubBusiness pubBusiness, 
            string mailToDefault)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _authenticUserModelList = authenticUserModelList;
            _mailToDefault = mailToDefault;
            HttpRequest httpRequest = context.HttpContext.Request;

            _httpHost = string.IsNullOrEmpty(httpRequest?.Host.Port.ToString())
                           ? $"{httpRequest?.Scheme}://{httpRequest?.Host.Host}"
                           : $"{httpRequest?.Scheme}://{httpRequest?.Host.Host}:{httpRequest?.Host.Port}";

            _pubBusiness = pubBusiness;
        }
         
        /// <summary>
        /// 邮件发送API接口（/MailService/SendMail）
        /// Token通过Header的Authorization: Bearer {token}传入，而不是參數。
        /// 提供第三方接入使用，而非本地。
        /// </summary>
        /// <param name="emailModel">邮件发送模型</param>
        /// <returns>发送结果响应</returns>
        [HttpPost("SendMail")]
        public async Task<IActionResult> SendMail([FromBody] SendingEmailModel emailModel)
        {
            _logger.LogInformation("{mainComId} MailService/SendMail called at {Time}", emailModel?.MainComId, DateTime.Now.ToLocalTime());

            // 初始化响应对象
            var response = new ResponseModalX
            {
                meta = new MetaModalX
                {
                    Success = false,
                    Message = "Email failed to send",
                    ErrorCode = 1
                },
                data = new { }
            };

            try
            {
                // 1. 基础参数验证
                if (emailModel == null)
                {
                    response.meta.Message = "Please provide valid email parameters.";
                    return Ok(response);
                }

                if (string.IsNullOrEmpty(emailModel.MainComId))
                {
                    response.meta.Message = "Reuired Parameter：mainComId";
                    return Ok(response);
                }
                emailModel.MainComId = emailModel.MainComId.Trim();

                if (string.IsNullOrEmpty(emailModel.LanguageCode))
                    emailModel.LanguageCode = "zh-HK";

                if(!string.IsNullOrEmpty(emailModel.EmailContent))
                {
                    emailModel.EmailContent = Base64Helper.TryDecodeBase64(emailModel.EmailContent); 
                }

                if (!string.IsNullOrEmpty(emailModel.CallbackUrlEncode))
                {
                    emailModel.CallbackUrlEncode = Uri.UnescapeDataString(emailModel.CallbackUrlEncode);
                }

                // 2. 验证mainComId是否有效
                _logger.LogInformation("_authenticUserModelList {Count}, mainComId = {mainComId}", _authenticUserModelList.Count(), emailModel.MainComId);
                AuthenticUserModel authenticUserModel = _authenticUserModelList.FirstOrDefault(u => u.MainComId.Contains(emailModel.MainComId))??null;

                if (authenticUserModel == null)
                {
                    response.meta.Message = "Invalid mainComId, no corresponding authenticated user information found.";
                    return Ok(response);
                }

                // 3. 从Header获取Authorization Token（仅支持Bearer方式）
                var authorizationHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authorizationHeader))
                {
                    response.meta.Message = "Please provide Authorization in the Header: Bearer {token}";
                    return Ok(response);
                }

                // 验证Bearer Token格式
                if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    response.meta.Message = "The authorization format is incorrect. Please use Bearer {token}.";
                    return Ok(response);
                }

                var token = authorizationHeader.Substring("Bearer ".Length).Trim();
                if (token != authenticUserModel.AuthenticationCode)
                {
                    response.meta.Message = $"Invalid Token：{token}";
                    return Ok(response);
                }

                // 4. 解析邮件模板
                MailTemplateEnum mailTemplate = MailTemplateEnum.NO_TEMPLATE;
                if (!string.IsNullOrEmpty(emailModel.MailTemplateEnum))
                {
                    if (!Enum.TryParse<MailTemplateEnum>(emailModel.MailTemplateEnum, out mailTemplate))
                    {
                        response.meta.Message = $"Invalid email template type：{emailModel.MailTemplateEnum},Or transmitted NO_TEMPLATE ";
                        return Ok(response);
                    }
                }

                // 5. 解析收件人列表
                //string[] mailToList;

                var mailToList = emailModel.Mailto.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(s => s.Trim())
                             .ToArray();
                 

                // 6. 初始化邮件服务并发送
                var emailAppService = EmailAppService.StartUpEmailAppService(authenticUserModel);
                emailAppService._logger.LogInformation("EmailAppService started for MailService/SendMail.");

                bool success = await emailAppService.RunAsync(
                    mailToList,
                    emailModel.Subject,
                    mailTemplate,
                    emailModel.EmailContent,
                    emailModel.LanguageCode,
                    emailModel.CallbackUrlEncode ?? null,
                    null
                );

                // 7. 处理发送结果
                if (success)
                {
                    response.meta.Success = true;
                    response.meta.Message = "The email has been sent successfully.";
                    response.meta.ErrorCode = 0;

                    // 備份 郵件內容 html 格式
                    string emailContentfileName = $"{DateTime.Now:yyy-MM-dd HH-mm}.html";
                    string pathFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"EmailLog",emailModel.MainComId, emailContentfileName);
                    System.IO.File.WriteAllText(pathFileName, emailModel.EmailContent, Encoding.UTF8);
                }
                else
                {
                    response.meta.Message = "Email failed to send, please check the log.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MailService/SendMail An error occurred while sending email.");
                response.meta.Message = $"Internal error：{ex.Message}";
                response.meta.ErrorCode = 500;
            }

            return Ok(response);
        }
         
    }
}
