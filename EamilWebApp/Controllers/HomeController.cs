using EamilWebApp.Models;
using MailEnhanceService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace EamilWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IList<AuthenticUserModel> _authenticUserModelList;
        private readonly string _mailToDefault;
        private string _httpHost { get; set; }

        // 公司用戶認證服務接口，通過依賴注入獲取實現類的實例
        private readonly IPubBusiness _pubBusiness;
        public HomeController(
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
            HttpRequest? httpRequest = context.HttpContext?.Request;

            _httpHost = string.IsNullOrEmpty(httpRequest?.Host.Port.ToString())
                           ? $"{httpRequest?.Scheme}://{httpRequest?.Host.Host}"
                           : $"{httpRequest?.Scheme}://{httpRequest?.Host.Host}:{httpRequest?.Host.Port}";


            _pubBusiness = pubBusiness;
        }

        public IActionResult Index()
        {

            _logger.LogInformation("Index action called at {Time}", DateTime.UtcNow.ToLocalTime());

            return View(_authenticUserModelList);
        }

        [HttpGet]
        public IActionResult Authorize(string mainComId)
        {
            _logger.LogInformation("Authorize Action MainComId={mainComId} called at {Time}", mainComId, DateTime.UtcNow);

            ViewBag.MainComId = mainComId;  // 視圖前端使用
            return View();
        }


        /// <summary>
        /// Email編輯器的頁面。
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult EmailEditor(string mainComId, string token)
        {
            // 准备响应对象
            var response = new ResponseModalX
            {
                meta = new MetaModalX
                {
                    Success = false,
                    Message = "Invalid user authentication failed!",
                    ErrorCode = 1
                },
                data = new { }
            };

            _logger.LogInformation("EmailEditor action http get at {Time}", DateTime.UtcNow.ToLocalTime());

            if (string.IsNullOrEmpty(mainComId))
            {
                return BadRequest(new { code = 1, msg = "no parameter input : mainComId" });
            }
             
            try
            { 
                // 參數 token 
                if (string.IsNullOrEmpty(token))
                {
                    response.meta.Message = "Token is empty after Bearer prefix.";
                    return View(response);
                }
                 
                // 3. 拼接完整的认证头（Bearer + token），匹配 IsValidAuth 方法的校验逻辑
                string authHeader = $"Bearer {token.Trim()}";
                // 4. 调用认证方法（传入完整的 authHeader）
                // 验证 token
                if (!_pubBusiness.IsValidAuth(mainComId, authHeader))
                {
                    response.meta.Message = $"Invalid token: {token}";
                    return View("ResponseModal", response);
                }
            }
            catch (Exception ex)
            {
                response.meta = new MetaModalX
                {
                    ErrorCode = (int)GeneralResult.EXCEPTION,
                    Success = false,
                    Message = $"Authentication exception: {ex.Message}"
                };
                return View(response);
            }


            ViewBag.MainComId = mainComId;  // 視圖前端使用
            ViewBag.AuthenticationCode = token;  // 視圖前端使用

            return View(response);
        }

        [HttpPost]
        public async Task<IActionResult> EmailEditor([FromBody] SendingEmailModel emailModel)
        {
            _logger.LogInformation("{mainComId} EmailEditor POST called at {Time}", emailModel.MainComId, DateTime.Now);

            if (string.IsNullOrEmpty(emailModel.LanguageCode))
                emailModel.LanguageCode = "zh-HK";

            MailTemplateEnum mailTemplate = MailTemplateEnum.NO_TEMPLATE;  // 每設計一個模版都會在此定義，目前有 REGISTER, FORGET_PASSWORD, PRIVACY_CONTENT 等等，這些模版會對應到 EmailAppService.GetMailTemplate() 方法裡的邏輯來決定使用哪一個 HTML 模版來發送郵件。
            if (!string.IsNullOrEmpty(emailModel.MailTemplateEnum))
            {
                mailTemplate = Enum.Parse<MailTemplateEnum>(emailModel.MailTemplateEnum);
            }

            // 准备响应对象
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
                if (!string.IsNullOrEmpty(emailModel.MainComId))
                    emailModel.MainComId = emailModel.MainComId.Trim();
                 
                _logger.LogInformation("_authenticUserModelList {Count}, mainComId = {mainComId}", _authenticUserModelList.Count(), emailModel.MainComId);
                AuthenticUserModel authenticUserModel = _authenticUserModelList.Where(u => u.MainComId.Contains(emailModel.MainComId)).FirstOrDefault() ;
                if(authenticUserModel == null)
                {
                    response.meta.Message = "無效的 mainComId，未找到對應的認證用戶信息";
                    return Ok(response);
                }

                // 判斷 AuthenticationCode 是否存在且有效
                var authorizationCode = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authorizationCode))
                {
                    response.meta.Message = "無效的 AuthenticationCode，請提供有效的認證碼";
                    return Ok(response);
                }

                if (authorizationCode != $"Bearer {authenticUserModel.AuthenticationCode}")
                {
                    response.meta.Message = $"傳入無效的AuthenticationCode：{authorizationCode}";
                    return Ok(response);
                }

                var emailAppService = EmailAppService.StartUpEmailAppService(authenticUserModel);
                emailAppService._logger.LogInformation("EmailAppService started.");

                // 解析收件人列表
                string[] mailToList;
                if (string.IsNullOrEmpty(emailModel.Mailto))
                    mailToList = new[] { _mailToDefault }; // 默认测试邮箱  "mcessol2000@gmail.com"
                else if (emailModel.Mailto.Contains(','))
                    mailToList = emailModel.Mailto.Split(',').Select(m => m.Trim()).ToArray();
                else
                    mailToList = new[] { emailModel.Mailto.Trim() };

                bool success = await emailAppService.RunAsync(
                    mailToList,
                    emailModel.Subject,
                    mailTemplate,  //MailTemplateEnum.NO_TEMPLATE,
                    emailModel.EmailContent,
                    emailModel.LanguageCode,
                    emailModel.CallbackUrlEncode ?? null,
                    null
                );

                if (success)
                {
                    response.meta.Success = true;
                    response.meta.Message = "郵件已成功發送";
                    response.meta.ErrorCode = 0;
                }
                else
                {
                    response.meta.Message = "郵件發送失敗，請檢查日誌";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送郵件時發生異常");
                response.meta.Message = $"內部錯誤：{ex.Message}";
                response.meta.ErrorCode = 500;
            }

            return Ok(response);
        }

        /// <summary>
        /// 上載圖片 
        /// /wwwroot/uploads/6000014公司ID/images/uniqueFileName.jpg
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UploadImg(string mainComId, string token, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { code = 1, msg = "No file uploaded." });
            }

            // SASS 多公司平台必須指定 mainComId 以便存储在对应的目录下
            if (string.IsNullOrEmpty(mainComId))
            {
                return BadRequest(new { code = 1, msg = "Invalid mainComId parameter input!" });
            }

            // token 認證
            if (string.IsNullOrEmpty(token) || !_pubBusiness.IsValidAuth(mainComId, $"Bearer {token}"))
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !_pubBusiness.IsValidAuth(mainComId, authHeader))
                    return Unauthorized(new { code = 401, msg = "Invalid authorization" });
            }

            // 1. 验证文件类型（确保是图片）
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { code = 1, msg = "Invalid file type. Only images are allowed." });
            }

            // 2. 生成安全的唯一文件名
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

            // 3. 设定存储路径：wwwroot/uploads/ （确保该目录存在）
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", mainComId, "images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 4. 保存文件
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 5. 构建可用于访问的 URL 
            var fileUrl = Url.Content($"{_httpHost}/uploads/{mainComId}/images/{uniqueFileName}");
            //lakejs  返回的格式要求 
            object logData = new { result = true, msg = "picture uploaded successfully", url = fileUrl };
            _logger.LogInformation(JsonConvert.SerializeObject(logData));
            return Ok(logData);
        }


        /// <summary>
        /// 上传media（视频、音频等）
        /// Set storage path: wwwroot/uploads/{mainComId}/media/
        /// </summary>
        /// <param name="mainComId">公司ID(MainComId)</param>
        /// <param name="file">上传的文件</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UploadMedia(string mainComId, string token, IFormFile file)
        {

            if (string.IsNullOrEmpty(mainComId))
            {
                return BadRequest(new { code = 1, msg = "no parameter input : mainComId" });
            }
             
            // token 認證
            if (string.IsNullOrEmpty(token) || !_pubBusiness.IsValidAuth(mainComId, $"Bearer {token}"))
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !_pubBusiness.IsValidAuth(mainComId, authHeader))
                    return Unauthorized(new { code = 401, msg = "Invalid authorization" });
            }

            // 1. 基礎驗證
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { code = 1, msg = "no file uploaded" });
            }

            // 2. Allowed media file types (can be adjusted as needed)
            var allowedMediaTypes = new Dictionary<string, string[]>
            {
                { "video", new[] { ".mp4", ".mpeg", ".webm", ".avi", ".mov", ".wmv" } },
                { "audio", new[] { ".mp3", ".wav", ".ogg", ".aac", ".flac" } }
            };
            var allowedExtensions = allowedMediaTypes.Values.SelectMany(v => v).ToArray();

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { code = 1, msg = "Unsupported file type. Please upload a video or audio file." });
            }

            // 3. (Optional) Limit file size, for example, maximum 300 MB
            const long maxFileSize = 300 * 1024 * 1024;  
            if (file.Length > maxFileSize)
            {
                return BadRequest(new { code = 1, msg = "The file size exceeds the limit (maximum 300 MB)." });
            }

            // 4. Generate unique filenames (preserving the original extension). 
            var uniqueFileName = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}_{Path.GetFileName(file.FileName)}";

            // 5. Set storage path: wwwroot/uploads/{mainComId}/media/
            var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", mainComId, "media");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            // 6. Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 7. Construct an accessible URL (note that the path includes the images subdirectory).
            var fileUrl = Url.Content($"{_httpHost}/uploads/{mainComId}/media/{uniqueFileName}");

            // lakejs 后端upload media已满足该结构（返回 { code:0, msg:"ok", url:"http://...."}）
            return Ok(new { code = 0, msg = "media uploaded successfully!", url = fileUrl });
        }

        /// <summary>
        /// 上傳一般文件附件（PDF、ZIP、Office文件等）
        /// </summary>
        /// <param name="mainComId">公司ID（必須）</param>
        /// <param name="file">上傳的檔案</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> UploadFile(string mainComId, string token, IFormFile file)
        {
            if (string.IsNullOrEmpty(mainComId))
            {
                return BadRequest(new { code = 1, msg = "no parameter input : mainComId" });
            }

            // token 認證
            if (string.IsNullOrEmpty(token) || !_pubBusiness.IsValidAuth(mainComId, $"Bearer {token}"))
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !_pubBusiness.IsValidAuth(mainComId, authHeader))
                    return Unauthorized(new { code = 401, msg = "Invalid authorization" });
            }
             
            // 1. 基礎驗證
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { code = 1, msg = "no file uploaded" });
            } 

            // 2. 允許的檔案副檔名（可依需求調整）
            var allowedExtensions = new[]
            {
                 ".pdf", ".zip", ".rar", ".7z", // 壓縮包
                 ".doc", ".docx", ".xls", ".xlsx", // office 文檔
                 ".ppt", ".pptx", ".txt", ".csv", // office 及文字
                 ".jpg", ".jpeg", ".png", ".gif", ".bmp" // 圖片（其實已有 UploadImg，但有需要也可支援）
                 // 可繼續新增其他類型
             };

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { code = 1, msg = "Unsupported file type. Please upload a valid file (PDF, ZIP, Office, etc.)." });
            }

            // 3. 限製檔案大小（例如最大 100 MB，可依需求調整）
            const long maxFileSize = 100 * 1024 * 1024; // 100 MB
            if (file.Length > maxFileSize)
            {
                return BadRequest(new { code = 1, msg = $"The file size exceeds the limit (maximum {maxFileSize / 1024 / 1024} MB)." });
            }

            // 4. 產生唯一檔案名稱（保留原始副檔名）
            var uniqueFileName = $"{DateTimeOffset.Now.ToUnixTimeMilliseconds()}_{Path.GetFileName(file.FileName)}";

            // 5. 設定儲存路徑：wwwroot/uploads/{mainComId}/files/
            var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", mainComId, "files");
            
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            // 6. 儲存文件
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 7. 建立可存取的 URL（相對路徑，或使用自訂的 HttpHost）
            var fileUrl = Url.Content($"{_httpHost}/uploads/{mainComId}/files/{uniqueFileName}");

            // lakejs 后端upload media已满足该结构（返回 { code:0, msg:"ok", url:"http://...."}）
            return Ok(new { code = 0, msg = "file uploaded successfully!", url = fileUrl });
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        /// <summary>
        /// 上传邮件模板（HTML文件）到 MailTemplate/{MainComId}/ 目录
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> UploadTemplate(string mainComId, string token, IFormFile file)
        {
            // 1. 参数验证
            if (string.IsNullOrEmpty(mainComId))
            {
                return BadRequest(new { code = 1, msg = "The necessary parameters are missing：mainComId" });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { code = 1, msg = "Please select the template file to upload" });
            }

            // 2. 文件类型限制（仅允许HTML及相关文本格式）
            var allowedExtensions = new[] { ".html", ".htm", ".txt", ".cshtml" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { code = 1, msg = "For unsupported file types, please upload a .html, .htm, or .txt file" });
            }

            // 3. 文件大小限制（5MB）
            const long maxFileSize = 5 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return BadRequest(new { code = 1, msg = $"The file size exceeds the limit（最大 {maxFileSize / 1024 / 1024} MB）" });
            }

            // 4. 认证验证（与现有上传方法保持一致）
            if (string.IsNullOrEmpty(token) || !_pubBusiness.IsValidAuth(mainComId, $"Bearer {token}"))
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrEmpty(authHeader) || !_pubBusiness.IsValidAuth(mainComId, authHeader))
                {
                    return Unauthorized(new { code = 401, msg = "Invalid authorization information，请检查 MainComId 和 Token" });
                }
            }

            try
            {
                // 5. 获取安全的文件名（保留原始文件名，防止路径遍历）
                var originalFileName = Path.GetFileName(file.FileName);
                var safeFileName = string.Concat(originalFileName.Split(Path.GetInvalidFileNameChars()));

                // 6. 构建存储路径：ContentRootPath/MailTemplate/{MainComId}/
                var templateFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "MailTemplate", mainComId);
                if (!Directory.Exists(templateFolder))
                {
                    Directory.CreateDirectory(templateFolder);
                }

                var filePath = Path.Combine(templateFolder, safeFileName);

                // 7. 保存文件（存在则覆盖）
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 8. 读取文件内容以便前端加载到编辑器
                string fileContent = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
                 
                _logger.LogInformation("The template was uploaded successfully：{FilePath}，MainComId：{MainComId}", filePath, mainComId);

                // 9. 返回成功信息及文件内容
                return Ok(new
                {
                    code = 0,
                    msg = "模板上傳成功！已自動加載到編輯器",
                    fileName = safeFileName,
                    content = fileContent,
                    path = $"/MailTemplate/{mainComId}/{safeFileName}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "上傳模板時發生異常，MainComId：{MainComId}", mainComId);
                return StatusCode(500, new { code = 500, msg = $"Upload failed：{ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult GetTemplateList(string mainComId, string token)
        {
            // 参数和认证验证（同上，略）
            if (string.IsNullOrEmpty(mainComId)) return BadRequest(new { code = 1, msg = "Miss mainComId" });
            // ... 认证验证代码 ...

            var templates = new List<object>();

            // 1. 公共模板：/MailTemplate/ 根目录下的文件（排除子文件夹）
            var rootTemplateDir = Path.Combine(_webHostEnvironment.ContentRootPath, "MailTemplate");
            if (Directory.Exists(rootTemplateDir))
            {
                // 只获取根目录下的文件，不包含子目录
                var rootFiles = Directory.GetFiles(rootTemplateDir, "*.*")
                    .Where(f => new[] { ".html", ".htm", ".txt" }.Contains(Path.GetExtension(f).ToLower()))
                    .Select(f => new
                    {
                        name = Path.GetFileName(f),
                        // 注意：path 参数只传文件名，不带路径前缀
                        url = Url.Action("GetTemplateContent", "Home", new { path = Path.GetFileName(f), mainComId, token }, Request.Scheme),
                        type = "public"
                    });
                foreach (var file in rootFiles)
                {
                    templates.Add(new { label = file.name, value = file.url, type = "public" });
                }
            }

            // 2. 商家模板：/MailTemplate/{mainComId}/ 子目录
            var companyTemplateDir = Path.Combine(_webHostEnvironment.ContentRootPath, "MailTemplate", mainComId);
            if (Directory.Exists(companyTemplateDir))
            {
                var companyFiles = Directory.GetFiles(companyTemplateDir, "*.*")
                    .Where(f => new[] { ".html", ".htm", ".txt" }.Contains(Path.GetExtension(f).ToLower()))
                    .Select(f => new
                    {
                        name = Path.GetFileName(f),
                        // 注意：path 参数传递 "MainComId/文件名" 格式
                        url = Url.Action("GetTemplateContent", "Home", new { path = $"{mainComId}/{Path.GetFileName(f)}", mainComId, token }, Request.Scheme),
                        type = "company"
                    });
                foreach (var file in companyFiles)
                {
                    templates.Add(new { label = file.name, value = file.url, type = "company" });
                }
            }

            return Ok(new { code = 0, data = templates });
        }

        [HttpGet]
        public async Task<IActionResult> GetTemplateContent(string path, string mainComId, string token)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(mainComId))
                return BadRequest(new { code = 1, msg = "Missing parameters" });

            // 认证验证（略）...

            // 安全校验：禁止路径遍历
            path = path.Replace("..", "").TrimStart('/');
            var allowedExtensions = new[] { ".html", ".htm", ".txt" };
            var extension = Path.GetExtension(path).ToLower();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { code = 1, msg = "Unsupported file types" });

            string fullPath;
            // 判断是公共模板（path 不包含 '/'）还是商家模板（path 包含 '/'）
            if (path.Contains('/'))
            {
                // 商家模板：格式 "MainComId/filename"
                var parts = path.Split('/');
                if (parts.Length != 2 || parts[0] != mainComId)
                    return BadRequest(new { code = 1, msg = "Path does not match merchant ID(MainComId)" });
                fullPath = Path.Combine(_webHostEnvironment.ContentRootPath, "MailTemplate", path);
            }
            else
            {
                // 公共模板：直接文件名，位于 MailTemplate 根目录
                fullPath = Path.Combine(_webHostEnvironment.ContentRootPath, "MailTemplate", path);
            }

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { code = 404, msg = "Template file does not exist" });

            try
            {
                var content = await System.IO.File.ReadAllTextAsync(fullPath, Encoding.UTF8);
                return Ok(new { code = 0, content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read template file：{Path}", fullPath);
                return StatusCode(500, new { code = 500, msg = "Failed to read template file" });
            }
        }
    }
}
