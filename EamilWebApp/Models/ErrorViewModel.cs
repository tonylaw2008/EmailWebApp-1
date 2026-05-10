using System.ComponentModel.DataAnnotations;

namespace EamilWebApp.Models
{
    public class ErrorViewModel
    {
        [Required]
        public string RequestId { get; set; } = Guid.NewGuid().ToString();

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
