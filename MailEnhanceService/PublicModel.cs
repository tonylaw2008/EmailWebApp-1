using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace MailEnhanceService
{ 
    public class AuthenticUserModel
    {
        [Required]
        public string MainComId { get; set; } = string.Empty;
        [Required]
        public string ShopId { get; set; } = string.Empty;
        [Required]
        public string UserName { get; set; } = string.Empty;
        [Required]
        public string AuthenticationCode { get; set; } = string.Empty;
        [Required]
        public string AuthenticMethods { get; set; } = "Bearer";
    }

    public class SendingEmailModel
    {
        [Required]
        public required string MainComId { get; set; }   

        [Required]
        public required string Mailto { get; set; }    

        [Required]
        public required string Subject { get; set; }

        [Required]
        public required string EmailContent { get; set; }

        [DefaultValue("")]
        public string CallbackUrlEncode { get; set; } = string.Empty;  

        [DefaultValue("zh-HK")]
        public string LanguageCode { get; set; } = "zh-HK";            

        [DefaultValue("NO_TEMPLATE")]
        public string MailTemplateEnum { get; set; } = "NO_TEMPLATE"; 
    }
     
}
