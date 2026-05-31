using MailKit;
//MailKit
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Runtime.Caching;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
 


namespace MailEnhanceService
{
    /// <summary>
    /// 允許簡單的標準化、普通化的模版： MailTemplateEnum 這個常量定義的模版可以直接使用模版，其他需要傳入郵件內容前，先生成內容。
    /// 常量與模版對應關係如下：
    /// ForgetPassword_zh-HK.html  去掉下劃線，單詞直接拼合一起並且：第一字符大楷其餘細楷
    /// </summary>
    public enum MailTemplateEnum
    {
        NO_TEMPLATE = -1, //不使用模版，直接傳入內容
        REGISTER = 0,
        FORGET_PASSWORD = 1,   
        PRIVACY_CONTENT = 3
    }

    /// <summary>
    /// 選擇不同的SMTP發送平台
    /// 邏輯上都是基於SMTP協議的發送方式，
    /// 但是考慮到不同平台接口的區別，分別獨立的4個函數，以靈活應對不同情況。 
    /// </summary>
    public enum MailToolEnum
    {
        SYSTEM_NET_MAIL_SMTP = 0,
        MAIL_KIT_SMTP = 1,
        QQ_SMTP = 2,
        OUTLOOK_SMTP = 3
    }

    /// <summary>
    /// SenderEmailAccount 類別用於儲存發送郵件的帳戶資訊的清單單元
    /// </summary>
    public class SenderEmailAccount : ISenderEmailAccount
    {
        /// <summary>
        /// 發件人公司名稱（例如：yahoo.hk, 163.com, 126.com, qq.com, gmail.com等）
        /// 域名部分
        /// </summary>
        public string SenderOfCompany { get; set; }

        /// <summary>
        /// 使用那個工具發送EMAIL:  0: System.Net.Mail 系統自帶, 1: MailKit
        /// </summary>
        public int MailTool { get; set; }
        

        /// <summary>
        /// 是否啟用 SSL（安全套接層）協議   
        /// </summary>
        public bool EnableSSL { get; set; }

        /// <summary>
        /// 是否啟用 TSL（傳輸層安全性）協議
        /// </summary>
        public bool EnableTSL { get; set; }
        public bool EnablePasswordAuthentication { get; set; }

        /// <summary>
        /// SMTP 伺服器主機地址
        /// </summary>
        public string SenderServerHost { get; set; }

        /// <summary>
        /// SMTP 伺服器主機端口號
        /// </summary>
        public int SenderServerHostPort { get; set; }

        /// <summary>
        /// 發件人郵箱地址
        /// </summary>
        public string FromMailAddress { get; set; }

        /// <summary>
        /// 發件人郵箱顯示名稱
        /// </summary>
        public string FromMailDisplayName { get; set; }
        
        /// <summary>
        /// SMTP 認證登錄賬號
        /// </summary>
        public string SenderUserName { get; set; }

        /// <summary>
        /// SMTP 認證登錄密碼
        /// </summary>
        public string SenderUserPassword { get; set; }

        /// <summary>
        /// 備註信息
        /// </summary>
        public string Remarks { get; set; }
    }
    public interface ISenderEmailAccount
    {
        public string SenderOfCompany { get; set; }
        public int MailTool { get; set; }
       
        public bool EnableSSL { get; set; }
        public bool EnableTSL { get; set; }
        public bool EnablePasswordAuthentication { get; set; }
        public string SenderServerHost { get; set; }
        public int SenderServerHostPort { get; set; }
        public string SenderUserName { get; set; }
        public string FromMailAddress { get; set; }
        public string FromMailDisplayName { get; set; }
        public string SenderUserPassword { get; set; }
        public string Remarks { get; set; }
    }

    public class MailMessageMain
    {
        public string? ToMail { get; set; }
        public string? ToMailDisplayName { get; set; }
        public string? Subject { get; set; }
        public string? EmailBody { get; set; } 
        public string[]? AttachedFiles { get; set; }
    }
    
    public class EmailEnhanceHelper
    {  
        private readonly ILogger<EmailEnhanceHelper> _logger;
        private readonly IMemoryCache _memoryCache;
        private string _mailComId { get; set; }
        private SenderEmailAccount? _senderAccount { get; set; }

        private MailMessageMain _mailMessageMain = new MailMessageMain();

        private MailMessage _mailMessage = new MailMessage();

        private MimeMessage _mailKitMessage = new MimeMessage();
       
        ///<summary>
        /// 构造函数
        ///</summary>
        ///<param name="server">发件箱的邮件服务器地址</param>
        ///<param name="toMail">收件人地址（可以是多个收件人，程序中是以“;"进行区分的）</param>
        ///<param name="fromMail">发件人地址</param>
        ///<param name="subject">邮件标题</param>
        ///<param name="emailBody">邮件内容（可以以html格式进行设计）</param>
        ///<param name="username">发件箱的用户名（即@符号前面的字符串，例如：hello@163.com，用户名为：hello）</param>
        ///<param name="password">发件人邮箱密码</param>
        ///<param name="port">发送邮件所用的端口号（htmp协议默认为25）</param>
        ///<param name="sslEnable">true表示对邮件内容进行socket层加密传输，false表示不加密</param>
        ///<param name="pwdCheckEnable">true表示对发件人邮箱进行密码验证，false表示不对发件人邮箱进行密码验证</param>
        public EmailEnhanceHelper(string mailComId, ILogger<EmailEnhanceHelper> logger, IMemoryCache memoryCache)
        {
            _mailComId = mailComId??string.Empty;
            _memoryCache = memoryCache;
            _logger = logger;
            _logger.LogInformation("EmailHelper initialized"); 
        }

        ///<summary>
        /// System.Net.Smtp 添加附件
        ///</summary>
        ///<param name="attachmentsPathFile">文件的詳細路徑和文件名</param>
        private void AddAttachments(string attachmentsPathFile)
        {
            if(!string.IsNullOrEmpty(attachmentsPathFile))
            {
                try
                {   
                    Attachment data = new Attachment(attachmentsPathFile, MediaTypeNames.Application.Octet);
                    System.Net.Mime.ContentDisposition disposition = data.ContentDisposition;
                    disposition.CreationDate = File.GetCreationTime(attachmentsPathFile);
                    disposition.ModificationDate = File.GetLastWriteTime(attachmentsPathFile);
                    disposition.ReadDate = File.GetLastAccessTime(attachmentsPathFile);
                    string senderOfCompany = _senderAccount.SenderOfCompany.ToLower(); 
                    _mailMessage.Attachments.Add(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _logger.LogError(string.Format("[func::AddAttachments] [PATH :{0} [MESSAGE]:{1}", attachmentsPathFile, ex.Message));
                }
            }  
        }

        /// <summary>
        /// MailKit 添加附件
        /// </summary>
        /// <param name="attachmentsPathFile"></param>
        private void AddMailKitAttachments(string attachmentsPathFile)
        {
            if (!string.IsNullOrEmpty(attachmentsPathFile) && File.Exists(attachmentsPathFile))
            {
                try
                {
                    var mailKitAttachment = new MimePart
                    {
                        Content = new MimeContent(File.OpenRead(attachmentsPathFile)),
                        ContentDisposition = new MimeKit.ContentDisposition(MimeKit.ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = Path.GetFileName(attachmentsPathFile)
                    };
                    _mailKitMessage.Attachments.Append(mailKitAttachment);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _logger.LogError(ex, $"[func::AddMailKitAttachments] [PATH :{attachmentsPathFile}] [MESSAGE]:{ex.Message}");
                }
            }
        }

        /// <summary>
        /// SmtpClient 發送單元
        /// Core of the email sending unit (default)
        /// </summary>
        /// <returns></returns>
        private async Task<bool> SendMailAsynchronous()
        {
            if (_senderAccount == null)
            {
                _logger.LogError("[func::SendMailKitAsynchronous] SenderAccount is null. Please initialize SenderAccount before sending email.");
                return false;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool sendSuccess = false;
            Exception? lastException = null;

            try
            {
                // Initialize mail message
                _mailMessage.From = new MailAddress(_senderAccount.FromMailAddress, _senderAccount.FromMailDisplayName);
                string receivedMail = _mailMessageMain.ToMail ?? "mcessol2000@gmail.com";  //一般情況不可能目標收件地址為非法地址。
                _mailMessage.To.Add(receivedMail);

                _mailMessage.Subject = _mailMessageMain.Subject;
                _mailMessage.Body = _mailMessageMain.EmailBody;
                _mailMessage.IsBodyHtml = true;
                _mailMessage.SubjectEncoding = Encoding.UTF8;
                _mailMessage.BodyEncoding = Encoding.UTF8;
                _mailMessage.Priority = MailPriority.High;
                _mailMessage.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;
                _mailMessage.Sender = new MailAddress(_senderAccount.FromMailAddress);

                using System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();
                // Set SMTP server information
                client.Host = _senderAccount.SenderServerHost;
                client.Port = _senderAccount.SenderServerHostPort; // 显式指定端口
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Timeout = 10000;  
                if(_mailMessageMain.AttachedFiles!=null)
                {
                    client.Timeout = 60000; // 60' timeout 

                    // 添加附件到郵件
                    foreach (var itemFile in _mailMessageMain.AttachedFiles)
                    {
                        if(File.Exists(itemFile))
                        { 
                            AddAttachments(itemFile);
                        }
                        else
                        {
                            _logger.LogWarning($"[func::SendMailAsynchronous] Attachment file not found: {itemFile}");
                        } 
                    }
                }

                client.UseDefaultCredentials = false; //不使用本機器NLM憑證

                //// 加密方式配置 Encryption method configuration
                //if (_senderAccount.EnableSSL)
                //{
                //    // SSL加密（通常對應埠465）
                //    client.EnableSsl = true; 
                //    // SMTP服務器需要客戶端載入PFX的SSL證書
                //    if (client.ClientCertificates != null && 1==5)
                //    {
                //        var certificate = LoadCertificate(_mailComId);
                //        if (certificate != null)
                //        {
                //            client.ClientCertificates.Add(certificate);
                //            _logger.LogInformation($"[func::SendMailAsynchronous] Loaded certificate for {_mailComId} successfully.");
                //        }
                //        else
                //        {
                //            _logger.LogWarning($"[func::SendMailAsynchronous] No valid certificate found for {_mailComId}. Please check your configuration.");
                //        }
                //    }
                //}
                //else if (_senderAccount.EnableTSL)
                //{
                //    // TLS加密（通常對應埠587）
                //    client.EnableSsl = true;

                //    // 設定TLS協定版本（如果需要） 一般都是 Tls12
                //    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //}


                // STARTTLS TLS加密（通常對應埠587）and ensure the SMTP server you are connecting to supports STARTTLS.
                client.EnableSsl = true;

                // 設定TLS協定版本（如果需要） 一般都是 Tls12
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                 
                // 僅使用SMTP LOGIN身份驗證 不使用本機器NLM憑證
                client.Credentials = new NetworkCredential(
                    _senderAccount.SenderUserName,
                    _senderAccount.SenderUserPassword
                );

                // Send email
                await client.SendMailAsync(_mailMessage);
                sendSuccess = true;
            }
            catch (SmtpException smtpEx)
            {
                lastException = smtpEx;
                // Handling SMTP-specific errors
                HandleSmtpException(smtpEx);
            }
            catch (WebException webEx)
            {
                lastException = webEx;
                // Network connection issues
                HandleNetworkException(webEx);
            }
            catch (Exception ex)
            {
                lastException = ex;
                // Other errors
                _logger.LogError(ex, "[func::SendMail] An unexpected error occurred while sending email.");
            }
            finally
            {
                sw.Stop();
                string status = sendSuccess ? "SUCCESS" : "FAILED";
                string errorInfo = lastException != null ? $"[ERROR: {lastException.GetType().Name}]" : "";

                string loggerLine = $" [func::SendMailAsynchronous] [TO:{_mailMessageMain.ToMail}] [{status}] [FROM:{_senderAccount.SenderUserName}] [Elapsed:{sw.ElapsedMilliseconds}ms] {errorInfo}";

                //Console.WriteLine(loggerLine);
                _logger.LogInformation(loggerLine);

                if (lastException != null)
                { 
                    _logger.LogError(lastException, "[func::SendMailAsynchronous] Email sending failure details.");
                }

                _mailMessage.Dispose();
            } 
            return sendSuccess;
        }

        /// <summary>
        /// MailKit 發送單元 
        /// Core of the email sending unit (default)
        /// </summary>
        /// <returns></returns>
        /// <summary>
        /// MailKit 發送單元 
        /// Core of the email sending unit (default)
        /// </summary>
        /// <returns></returns>
        private async Task<bool> SendMailKitAsynchronous()
        {
            _logger.LogInformation("[func::SendMailKitAsynchronous] Tool: MailKit to send SMPT email or duksend api to send email.");

            if (_senderAccount == null)
            {
                _logger.LogError("[func::SendMailKitAsynchronous] SenderAccount is null. Please initialize SenderAccount before sending email.");
                return false;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool sendSuccess = false;
            Exception? lastException = null;

            try
            {
                // 初始化MailKit郵件訊息
                _mailKitMessage.From.Add(new MailboxAddress(
                _senderAccount.FromMailDisplayName,
                _senderAccount.FromMailAddress));

                // 新增收件者
                MailboxAddress receiver = new MailboxAddress(
                _mailMessageMain.ToMailDisplayName ?? _mailMessageMain.ToMail,
                _mailMessageMain.ToMail);
                _mailKitMessage.To.Add(receiver);

                _mailKitMessage.Subject = _mailMessageMain.Subject;
                _mailKitMessage.Priority = MessagePriority.Normal;
                _mailKitMessage.Sender = new MailboxAddress(_senderAccount.FromMailDisplayName, _senderAccount.FromMailAddress);

                // 新增附件到郵件
                if (_mailMessageMain.AttachedFiles != null && _mailMessageMain.AttachedFiles.Length > 0)
                {
                    foreach (var itemFile in _mailMessageMain.AttachedFiles)
                    {
                        if (File.Exists(itemFile))
                        {
                            AddMailKitAttachments(itemFile);
                            _logger.LogInformation($"[func::SendMailKitAsynchronous] Attachment file: {itemFile}");
                        }
                        else
                        {
                            _logger.LogWarning($"[func::SendMailKitAsynchronous] Attachment file not found: {itemFile}");
                        }
                    }
                }

                // 設置郵件正文
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = _mailMessageMain.EmailBody;
                bodyBuilder.TextBody = GetHtmlText(_mailMessageMain.EmailBody ?? string.Empty);
                _mailKitMessage.Body = bodyBuilder.ToMessageBody();

                using MailKit.Net.Smtp.SmtpClient mailKitClient = new MailKit.Net.Smtp.SmtpClient();

                // 新增伺服器憑證驗證回傳 accept all SSL certificates（可選但建議） 
                // 優化後的伺服器憑證驗證回調
                // 一般目前的郵局服務都具有SSL證書
                mailKitClient.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                {
                    // 1. 1. 邊界處理：伺服器未傳回憑證（極端情況）
                    if (certificate == null)
                    {
                        _logger.LogError("[ServerCertificateValidationCallback] 服务器未提供SSL证书，无法建立安全连接");
                        return false; // 無證書直接拒絕
                    }

                    // 2. 基礎資訊日誌（無論是否有錯誤，先記錄證書關鍵訊息，以便排查）
                    var certLog = new
                    {
                        Subject = certificate.Subject,
                        Issuer = certificate.Issuer,
                        Thumbprint = certificate.GetCertHashString(), // 證書指紋（唯一識別）
                        ValidFrom = certificate.GetEffectiveDateString(), // 有效期限起始
                        ValidTo = certificate.GetExpirationDateString(), // 有效期限結束
                        SslErrors = sslPolicyErrors.ToString()
                    };
                    _logger.LogInformation("[ServerCertificateValidationCallback] 服务器证书信息：{@CertInfo}", certLog);

                    // 3. 無錯誤場景：直接通過驗證
                    if (sslPolicyErrors == SslPolicyErrors.None)
                    {
                        // 驗證憑證是否有效
                        if (DateTime.Parse(certificate.GetExpirationDateString()) < DateTime.Now)
                        {
                            _logger.LogError("[ServerCertificateValidationCallback] 服务器证书已过期（有效期：{ValidFrom} ~ {ValidTo}）",
                                certLog.ValidFrom, certLog.ValidTo);
                            return false;
                        }
                         
                        _logger.LogInformation("[ServerCertificateValidationCallback] 服务器证书验证通过（无错误且有效期正常）");
                        return true;
                    }

                    // 4. 有錯誤場景：按錯誤類型細分處理（結合配置策略）
                    _logger.LogError("[ServerCertificateValidationCallback] 服务器证书验证失败，错误类型：{SslErrors}", sslPolicyErrors);


                    // 4.2 非嚴格模式：僅允許特定類型的錯誤（如測試環境允許自簽名）
                    switch (sslPolicyErrors)
                    {
                        // 场景1：证书链错误（常见于自签名证书，或根证书未安装）
                        //case SslPolicyErrors.RemoteCertificateChainErrors:
                        //    // 检查是否允许自签名证书  _senderAccount.IsAllowSelfSignedCertificate=沒有此參數定義
                        //    if (_senderAccount.IsAllowSelfSignedCertificate)
                        //    {
                        //        // 验证证书链是否为自签名（简单判断：颁发者 == 主题）
                        //        bool isSelfSigned = string.Equals(certificate.Issuer, certificate.Subject, StringComparison.OrdinalIgnoreCase);
                        //        if (isSelfSigned)
                        //        {
                        //            _logger.LogWarning("[ServerCertificateValidationCallback] 允许自签名证书（测试环境专用），跳过证书链错误");
                        //            return true;
                        //        }
                        //        _logger.LogError("[ServerCertificateValidationCallback] 非自签名证书的证书链错误，不允许跳过（需安装根证书）");
                        //        return false;
                        //    }
                        //    _logger.LogError("[ServerCertificateValidationCallback] 未启用「允许自签名」，拒绝证书链错误的证书");
                        //    return false;

                        // 场景2：证书名称不匹配（通常是服务器地址与证书主题不一致，如用IP访问但证书是域名）
                        case SslPolicyErrors.RemoteCertificateNameMismatch:
                            _logger.LogError("[ServerCertificateValidationCallback] 证书名称与服务器地址不匹配（可能是访问地址错误，或证书配置错误）");
                            return false; // 名称不匹配风险高，即使非严格模式也拒绝

                        // 场景3：服务器未提供证书（已在步骤1处理，此处冗余防止枚举新增）
                        case SslPolicyErrors.RemoteCertificateNotAvailable:
                            _logger.LogError("[ServerCertificateValidationCallback] 服务器未提供有效的SSL证书");
                            return false;

                        // 其他未定义错误：默认拒绝
                        default:
                            _logger.LogError("[ServerCertificateValidationCallback] 未知的证书验证错误，拒绝连接");
                            return false;
                    }
                };
                // 關鍵修復：根據SSL/TLS配置選擇正確的SecureSocketOptions
                SecureSocketOptions secureOptions;
                if (_senderAccount.EnableSSL)
                {
                    // SSL加密（通常對應埠465）
                    secureOptions = SecureSocketOptions.SslOnConnect;

                    // 載入PFX格式的憑證（客戶端憑證）
                    var certificate = LoadCertificate(_mailComId);

                    if (certificate != null)
                    {
                        // 某些伺服器(指Brevo.com)可能需要客戶端 SSL 憑證才能允許使用者連線。 
                        // 載入PFX格式的憑證。 
                        // 而不是指目標收件人的服務器要求SSL證書。 
                        if (mailKitClient.ClientCertificates != null)
                        {
                            mailKitClient.ClientCertificates.Add(certificate);
                            _logger.LogInformation($"★★★★★[func::SendMailKitAsynchronous] Loaded client certificate for {_mailComId}: " +
                            $"Subject={certificate.Subject}, Thumbprint={certificate.Thumbprint}, " +
                            $"HasPrivateKey={certificate.HasPrivateKey}");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"[func::SendMailKitAsynchronous] No valid certificate found for {_mailComId}.");
                    }

                    // 明確指定SSL/TLS協定版本（可選）
                    mailKitClient.SslProtocols = System.Security.Authentication.SslProtocols.Tls11 |
                         System.Security.Authentication.SslProtocols.Tls12 |
                         System.Security.Authentication.SslProtocols.Tls13;
                }
                else if (_senderAccount.EnableTSL)
                {
                    // STARTTLS加密（通常對應埠587）
                    secureOptions = SecureSocketOptions.StartTls;

                    // 明確指定TLS協定版本（可選）
                    mailKitClient.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                }
                else
                {
                    // 不使用任何安全性協定（極端情況）
                    secureOptions = SecureSocketOptions.None;
                    _logger.LogWarning("[func::SendMailKitAsynchronous] Using unencrypted connection. This is not recommended for production.");
                }

                // 連接SMTP伺服器（使用動態運算的加密選項）
                _logger.LogInformation($"[func::SendMailKitAsynchronous] Connecting to {_senderAccount.SenderServerHost}:{_senderAccount.SenderServerHostPort} with {secureOptions}");

                await mailKitClient.ConnectAsync(
                _senderAccount.SenderServerHost,
                _senderAccount.SenderServerHostPort,
                secureOptions);

                _logger.LogInformation($"[func::SendMailKitAsynchronous] Connected successfully. SSL/TLS established: {mailKitClient.IsSecure}");

                // 身份驗證
                if (_senderAccount.EnablePasswordAuthentication)
                {
                    _logger.LogInformation($"[func::SendMailKitAsynchronous] Authenticating as {_senderAccount.SenderUserName}");

                    await mailKitClient.AuthenticateAsync(
                    _senderAccount.SenderUserName,
                    _senderAccount.SenderUserPassword);

                    _logger.LogInformation("[func::SendMailKitAsynchronous] Authentication successful");
                }

                // 傳送郵件
                _logger.LogInformation("[func::SendMailKitAsynchronous] Sending email...");
                await mailKitClient.SendAsync(_mailKitMessage);

                // 斷開連接
                await mailKitClient.DisconnectAsync(true);

                _logger.LogInformation("[func::SendMailKitAsynchronous] Email sent successfully");
                sendSuccess = true;
            }
            catch (MailKit.Net.Smtp.SmtpCommandException exCommand)
            {
                lastException = exCommand;
                HandleMailKitSmtpException(exCommand.ErrorCode);
            }
            catch (MailKit.Security.SslHandshakeException sslEx)
            {
                lastException = sslEx;
                _logger.LogError(sslEx, $"[SslHandshakeException] SSL/TLS握手失敗。請檢查連接埠與加密方式是否符合（連接埠587通常用STARTTLS，465用SSL）");
            }
            catch (MailKit.Security.AuthenticationException authEx)
            {
                lastException = authEx;
                _logger.LogError(authEx, $"[AuthenticationException] {_senderAccount.SenderServerHost} 驗證失敗，請檢查使用者名稱{_senderAccount.SenderUserName}和密碼{_senderAccount.SenderUserPassword}");
            }
            catch (System.Net.Sockets.SocketException socketEx)
            {
                lastException = socketEx;
                _logger.LogError(socketEx, $"[SocketException] 網路連線錯誤: {socketEx.SocketErrorCode}");
            }
            catch (WebException webEx)
            {
                lastException = webEx;
                HandleNetworkException(webEx);
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogError(ex, "[func::SendMailKitAsynchronous] 發生意外錯誤");
            }
            finally
            {
                sw.Stop();
                string status = sendSuccess ? "SUCCESS" : "FAILED";
                string errorInfo = lastException != null ? $"[ERROR: {lastException.GetType().Name}]" : "";

                string loggerLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [MailKit] [TO:{_mailMessageMain.ToMail}] " +
                $"[FROM:{_senderAccount.FromMailAddress}] [{status}] [Elapsed:{sw.ElapsedMilliseconds}ms] {errorInfo}";

                // Console.WriteLine(loggerLine);
                _logger.LogInformation(loggerLine);

                if (lastException != null)
                {
                    _logger.LogError(lastException, "[func::SendMailKitAsynchronous] 郵件傳送失敗詳情");
                }

                // 清理資源
                _mailMessage.Dispose();
                _mailKitMessage.Dispose();
            }
            return sendSuccess;
        }

        /// <summary>
        /// SendQQAsynchronous 專用發送單元
        /// 其實和MailKit的發送單元邏輯基本一致，
        /// 主要區別在於：SendQQAsynchronous 可以使用API KEY的方式進行身份驗證， 或者直接使用SMTP協議方式發送。 
        /// Core of the email sending unit (default)
        /// </summary>
        private async Task<bool> SendQQAsynchronous()
        {
            _logger.LogInformation("[func::SendQQAsynchronous] 发送QQ邮箱（SMTP）邮件");

            if (_senderAccount == null)
            {
                _logger.LogError("[func::SendQQAsynchronous] SenderAccount 不能为空");
                return false;
            }

            // 强制适配QQ邮箱SMTP配置（也可从SenderAccount读取，建议配置化）
            //_senderAccount.SenderServerHost = "smtp.qq.com";
            //_senderAccount.SenderServerHostPort = _senderAccount.EnableSSL ? 465 : 587;
            //_senderAccount.EnableSSL = _senderAccount.SenderServerHostPort == 465;
            //_senderAccount.EnableTSL = _senderAccount.SenderServerHostPort == 587;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool sendSuccess = false;
            Exception? lastException = null;

            try
            {
                // 初始化QQ邮箱专属MimeMessage（逻辑同通用MailKit，但需注意发件人必须与QQ账号一致）
                _mailKitMessage.From.Add(new MailboxAddress(_senderAccount.FromMailDisplayName, _senderAccount.FromMailAddress));
                _mailKitMessage.To.Add(new MailboxAddress(_mailMessageMain.ToMailDisplayName ?? _mailMessageMain.ToMail, _mailMessageMain.ToMail));
                _mailKitMessage.Subject = _mailMessageMain.Subject;

                // 正文（QQ邮箱支持HTML，无需特殊处理）
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = _mailMessageMain.EmailBody;
                bodyBuilder.TextBody = GetHtmlText(_mailMessageMain.EmailBody ?? string.Empty);
                _mailKitMessage.Body = bodyBuilder.ToMessageBody();

                // 附件逻辑（复用通用AddMailKitAttachments）
                if (_mailMessageMain.AttachedFiles != null)
                {
                    foreach (var file in _mailMessageMain.AttachedFiles)
                    {
                        if (File.Exists(file)) AddMailKitAttachments(file);
                    }
                }

                using var mailKitClient = new MailKit.Net.Smtp.SmtpClient();
                // QQ邮箱证书验证（保留通用逻辑）
                mailKitClient.ServerCertificateValidationCallback = (sender, cert, chain, errors) =>
                {
                    // 生产环境建议严格验证，测试环境可简化
                    if (errors == SslPolicyErrors.None) return true;

                    _logger.LogError($"QQ郵箱證書驗證失敗：{errors}");
                    Console.WriteLine($"QQ郵箱證書驗證失敗：{errors}");

                    // 載入PFX格式的憑證（客戶端憑證）
                    var certificate = LoadCertificate(_mailComId);

                    if (certificate != null)
                    {
                        // 某些伺服器(指Brevo.com)可能需要客戶端 SSL 憑證才能允許使用者連線。 
                        // 載入PFX格式的憑證。 
                        // 而不是指目標收件人的服務器要求SSL證書。 
                        if (mailKitClient.ClientCertificates != null)
                        {
                            mailKitClient.ClientCertificates.Add(certificate);
                            string importantHints = $"🌄 [IMPORTANT] [MAIL TASK SERVICE] 🎡 [★★★★★][func::SendQQAsynchronous] Loaded client certificate for {_mailComId}: "
                                                     + $"Subject={certificate.Subject}, Thumbprint={certificate.Thumbprint}, "
                                                     + $"HasPrivateKey={certificate.HasPrivateKey}";
                            _logger.LogInformation(importantHints);
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"[func::SendQQAsynchronous] No valid certificate found for {_mailComId}.");
                    }

                    return false;
                };

                // QQ邮箱SMTP连接（适配端口和加密方式）
                var secureOptions = _senderAccount.EnableSSL
                    ? SecureSocketOptions.SslOnConnect  // 465端口用SSL
                    : SecureSocketOptions.StartTls;    // 587端口用STARTTLS
                await mailKitClient.ConnectAsync(
                    _senderAccount.SenderServerHost,
                    _senderAccount.SenderServerHostPort,
                    secureOptions
                );

                // QQ邮箱认证：必须使用“授权码”而非登录密码，需提示用户在QQ邮箱设置中开启
                await mailKitClient.AuthenticateAsync(_senderAccount.SenderUserName, _senderAccount.SenderUserPassword);

                // 发送邮件
                await mailKitClient.SendAsync(_mailKitMessage);
                await mailKitClient.DisconnectAsync(true);

                sendSuccess = true;
                string sendMailResponse = $"\n🌄 [SEND MAIL RESPONSE SUCCESS] 🎡 [func::EmailEnhanceHelper.SendQQAsynchronous] [qq email sent successfully] [{_senderAccount.SenderUserName}] 🌇\n";
                _logger.LogInformation(sendMailResponse);
                Console.WriteLine(sendMailResponse);
            }
            catch (AuthenticationException ex)
            {
                lastException = ex;
                // QQ邮箱认证失败专属提示（授权码错误/未开启SMTP）
                string sendMailResponse = $"\n 🌄🎡 [func::EmailEnhanceHelper.SendQQAsynchronous] 🌄 [Send Mail Response AuthenticationException] [{ex.Message}] 🌇\n"
                                           + $"[qq email Authentication failed] Please Check Smtp：1.senderUserName and senderUserPassword 2. Is an authorization code being used instead of a login password?\r\n3. Are the username and password correct? [Check logs]";
                _logger.LogError(ex, sendMailResponse);
                await Console.Out.WriteLineAsync(sendMailResponse);
            }
            catch (SmtpCommandException ex)
            {
                lastException = ex;
                _logger.LogError(ex, $"[QQ Mail SMTP command error] Error code：{ex.ErrorCode} {ex.Message}");
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logger.LogError(ex, "[func::SendQQAsynchronous] Send failed");
            }
            finally
            {
                sw.Stop();
                // 日志记录（标准化）
                var status = sendSuccess ? "SUCCESS" : "FAILED";
                string sendMailFinallyMsg = $"\n 🌄🎡 [func::EmailEnhanceHelper.SendQQAsynchronous] [TO:{_mailMessageMain.ToMail}] [{status}] [Time Consuming:{sw.ElapsedMilliseconds}ms]🌇\n";
                _logger.LogInformation(sendMailFinallyMsg);
                await Console.Out.WriteLineAsync(sendMailFinallyMsg);
                // 资源释放
                _mailKitMessage.Dispose();
            }

            return sendSuccess;
        }
        /// <summary>
        /// 異步發送郵件 Sending emails asynchronously
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SendMailsync(SenderEmailAccount senderAccount,string toMail, string subject, string emailBody, string[]? attachedFiles=null)
        {
            //不能同時使用兩種加密協議： TLS/SSL
            if (senderAccount.EnableSSL && senderAccount.EnableTSL)
            {
                _logger.LogError("[func::SendMailsync] Cannot enable both SSL and TSL at the same time. Please check your configuration.");
                return false;
            }

            // 傳入發送單元的 發送賬號信息
            this._senderAccount = senderAccount;

            _mailMessageMain = new MailMessageMain { ToMail = toMail, Subject = subject, EmailBody = emailBody, AttachedFiles = attachedFiles };

            bool sendSuccess = false;

            string senderOfCompany = _senderAccount.SenderOfCompany.ToLower();

            await Task.Run(() =>
            { 
                _logger.LogInformation($"[Ready To Send Mail......] [SenderOfCompany: {senderOfCompany}] [ToMail: {toMail}][Subject: {subject}]");
            });

            try
            {
                if (_senderAccount.MailTool == (int)MailToolEnum.MAIL_KIT_SMTP) // MailKit
                {
                    sendSuccess = await SendMailKitAsynchronous();
                }else if (_senderAccount.MailTool == (int)MailToolEnum.QQ_SMTP) // dukesend.com
                {
                    sendSuccess = await SendQQAsynchronous();
                }
                else // System.Net.Mail
                {
                    sendSuccess = await SendMailAsynchronous();
                }
                 
                return sendSuccess;
            }
            catch
            {
                return sendSuccess;
            }
        }

        // 處理SMTP特定錯誤
        private void HandleSmtpException(SmtpException ex)
        {
            switch (ex.StatusCode)
            {
                case System.Net.Mail.SmtpStatusCode.GeneralFailure:
                    _logger.LogError(ex, $"[SmtpException] SMTP server connection failed, please check the server address and port {ex.StatusCode} {ex.Message}");
                    break;
                case System.Net.Mail.SmtpStatusCode.ClientNotPermitted:
                case System.Net.Mail.SmtpStatusCode.MustIssueStartTlsFirst:
                    _logger.LogError(ex, "SMTP authentication failed, please check your username and password");
                    break;
                case System.Net.Mail.SmtpStatusCode.MailboxBusy:
                case System.Net.Mail.SmtpStatusCode.InsufficientStorage:
                    _logger.LogWarning(ex, "The SMTP server is temporarily unavailable. Please try again later.");
                    break;
                default:
                    _logger.LogError(ex, $"SMTP ERROR: {ex.StatusCode}");
                    break;
            }
        }

        // 處理MailKit SMTP特定錯誤
        private void HandleMailKitSmtpException(MailKit.Net.Smtp.SmtpErrorCode exCode)
        {
            switch (exCode)
            {
                case MailKit.Net.Smtp.SmtpErrorCode.SenderNotAccepted:
                    _logger.LogError($"[MailKit.Net.Smtp.SmtpErrorCode.SenderNotAccepted {MailKit.Net.Smtp.SmtpErrorCode.SenderNotAccepted}][From:{_senderAccount?.FromMailAddress ?? string.Empty}] A recipient's mailbox address was not accepted. Check the SmtpCommandException.Mailbox");
                    break;
                case MailKit.Net.Smtp.SmtpErrorCode.UnexpectedStatusCode:
                    _logger.LogError($"[MailKit.Net.Smtp.SmtpErrorCode.UnexpectedStatusCode {MailKit.Net.Smtp.SmtpErrorCode.UnexpectedStatusCode}][From:{_senderAccount?.FromMailAddress ?? string.Empty}]");
                    break;
                case MailKit.Net.Smtp.SmtpErrorCode.RecipientNotAccepted:
                    _logger.LogError($"[MailKit.Net.Smtp.SmtpErrorCode.RecipientNotAccepted {MailKit.Net.Smtp.SmtpErrorCode.RecipientNotAccepted}][From:{_senderAccount?.FromMailAddress??string.Empty}]");
                    break;
                case MailKit.Net.Smtp.SmtpErrorCode.MessageNotAccepted:
                    _logger.LogError($"[MailKit.Net.Smtp.SmtpErrorCode.MessageNotAccepted {MailKit.Net.Smtp.SmtpErrorCode.MessageNotAccepted}][From:{_senderAccount?.FromMailAddress ?? string.Empty}]");
                    break; 
                default:
                    _logger.LogError($"[SmtpErrorCode] MailKit.Net.Smtp.SmtpErrorCode : {exCode}[From:{_senderAccount?.FromMailAddress ?? string.Empty}]");
                    break;
            }
        }
        // 處理網路連線錯誤
        private void HandleNetworkException(WebException ex)
        {
            switch (ex.Status)
            {
                case WebExceptionStatus.ConnectFailure:
                    _logger.LogError(ex, "無法連線到SMTP伺服器，請檢查網路連線 Unable to connect to the SMTP server, please check your network connection");
                    break;
                case WebExceptionStatus.Timeout:
                    _logger.LogError(ex, "連接SMTP伺服器逾時，請檢查伺服器狀態或增加逾時時間 The connection to the SMTP server has timed out. Please check the server status or increase the timeout period.");
                    break;
                case WebExceptionStatus.NameResolutionFailure:
                    _logger.LogError(ex, "無法解析SMTP伺服器位址，請檢查主機名稱是否正確 Unable to resolve the SMTP server address, please check if the host name is correct");
                    break;
                default:
                    _logger.LogError(ex, $"網路錯誤 Network Error: {ex.Status}");
                    break;
            }
        }

        //SecureString 是 C# 中用於增強敏感資料記憶體安全性的專用類型，適合處理需要暫時儲存且需嚴格保護的資訊。
        private SecureString SecureStringConverter(string pass)
        {
            SecureString ret = new SecureString();

            foreach (char chr in pass.ToCharArray())
                ret.AppendChar(chr);

            return ret;
        }

        /// <summary>
        /// Get plain text in HTML ref  https://blog.csdn.net/fuzhixin0/article/details/52129253
        /// </summary>
        /// <param name="html">html</param>
        /// <returns>plain text</returns>
        public string GetHtmlText(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return string.Empty;
            }
            html = System.Text.RegularExpressions.Regex.Replace(html, @"<\/*[^<>]*>", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            html = html.Replace("\r\n", "").Replace("\r", "").Replace("&nbsp;", "").Replace(" ", "");
            return html;
        }

        // 載入PFX格式的證書
        private X509Certificate2? LoadCertificate(string? mainComId)
        {
            mainComId = mainComId ?? string.Empty;  
            string CacheKeyOfX509Certificate2 = $"X509Certificate2CacheKey{mainComId}";

            if (!_memoryCache.TryGetValue(CacheKeyOfX509Certificate2, out X509Certificate2 certificate))
            { 
                //_memoryCache
                string pathOfCer = Path.Combine(AppContext.BaseDirectory, "Cer", "xguard_hk.pfx");
                string textOfCerPassword = string.Empty ;
                // 預設的憑證密碼
                string PathOfCerPassword = Path.Combine(AppContext.BaseDirectory, "Cer", "xguard_hk_password.txt");
                if (File.Exists(PathOfCerPassword))
                {
                    textOfCerPassword = File.ReadAllText(PathOfCerPassword).Trim();
                }

                try
                {
                    // 如指定了 mainComId，則使用對應的憑證路徑
                    if (!string.IsNullOrEmpty(mainComId))
                    {
                        // 如果有指定 mainComId，則使用對應的憑證路徑
                        string pathOfMainComCer = Path.Combine(AppContext.BaseDirectory, "Cer", mainComId, $"{mainComId}.pfx");
                        string pathOfMainComPassword = Path.Combine(AppContext.BaseDirectory, "Cer", mainComId, $"{mainComId}_password.txt");

                        _logger.LogInformation($"[func::LoadCertificate] [cer] pathOfMainComCer: {pathOfMainComCer} and  pathOfMainComPassword={pathOfMainComPassword}");

                        if (File.Exists(pathOfMainComCer) && File.Exists(pathOfMainComPassword))
                        {
                            //使用公司特定的憑證路徑
                            pathOfCer = pathOfMainComCer;
                            textOfCerPassword = File.ReadAllText(pathOfMainComPassword).Trim(); 
                        }
                        else
                        {
                            _logger.LogError($"[func::LoadCertificate] 憑證檔案不存在: {pathOfMainComCer} and {pathOfMainComPassword}");
                        }
                    }

                    // 載入證書
                    certificate = new X509Certificate2(pathOfCer, textOfCerPassword);

                    // 驗證憑證是否有效
                    if (certificate.NotBefore > DateTime.Now || certificate.NotAfter < DateTime.Now)
                    {
                        _logger.LogError("[func::LoadCertificate] 憑證已過期或尚未生效");
                        return null;
                    }

                    // Cache
                    _memoryCache.Set(CacheKeyOfX509Certificate2, certificate, TimeSpan.FromHours(72));

                    return certificate;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[func::LoadCertificate] 載入憑證時發生錯誤: {ex.Message}");
                    return null;
                }
            }
#if DEBUG
            Console.WriteLine($"[func::LoadCertificate] 使用緩存(memoryCache)的憑證: {CacheKeyOfX509Certificate2}");
#endif

            return certificate;
        }
    }

    /// <summary>
    /// 獲取 EMAIL SENDER ACCOUNT LIST 配置
    /// </summary>
    public static class EmailConfigHelper
    {
        /// <summary>
        /// 獲取 EMAIL SERVER AUTHENTIC USER 配置，從 Config 文件夾下對應店鋪的 JSON 文件讀取
        /// EmailWebAppService_{mainComId}.json
        /// </summary>
        /// <param name="mainComId"></param>
        /// <returns></returns>
        public static AuthenticUserModel LoadFromConfig(string mainComId)
        {
            if (string.IsNullOrEmpty(mainComId))
            {
                // 使用內置 項目 MailEnhanceService.csproj 發郵件 無需要
                // 無須配置 第三方發郵件(EamilWebApp.csproj)
                AuthenticUserModel authenticUserModel = new AuthenticUserModel
                {
                    MainComId = string.Empty,
                    ShopId = string.Empty,
                    UserName = string.Empty,
                    AuthenticationCode = string.Empty,
                    AuthenticMethods = string.Empty
                };
                return authenticUserModel;
            }
            else
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", $"EmailWebAppService_{mainComId}.json");
                string jsonData = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<AuthenticUserModel>(jsonData) ?? new AuthenticUserModel();
            }


        }

        /// <summary>
        /// 從本地文件夾 Config 下讀取 EMAIL 發件人帳號配置。
        /// 若果 SenderEmailAccountList_{mainComId}.json 文件不存在，則返回 SenderEmailAccountList.Json 帳號列表。
        /// </summary>
        /// <param name="mainComId"></param>
        /// <param name="senderListOfMainCom"></param>
        /// <returns></returns>
        public static bool ToEmailAccountsInstance(string mainComId, out IList<SenderEmailAccount> senderListOfMainCom)
        {
            senderListOfMainCom = new List<SenderEmailAccount>();

            string configPath = Path.GetFullPath("./Config");
            string configFileName = $"SenderEmailAccountList.Json";
            if (!string.IsNullOrEmpty(mainComId))
            {
                configFileName = $"SenderEmailAccountList_{mainComId}.json";
            }
            string pathfileName = Path.Combine(configPath, configFileName);

            if (!File.Exists(pathfileName))
            {
                // If the file does not exist, returns an empty list. 
                return false;
            }

            string jsonOfMailSenderList = MailCommonBase.ReadConfigJson(pathfileName);
            if (!string.IsNullOrEmpty(jsonOfMailSenderList))
            {
                senderListOfMainCom = JsonConvert.DeserializeObject<IList<SenderEmailAccount>>(jsonOfMailSenderList);

                if (senderListOfMainCom != null && senderListOfMainCom.Count > 0)
                {
                    return true; // If the list is empty after deserialization, return false
                }
            }
            return false;
        }
    }
}

