using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.DTO
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Current password is required!")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required!")]
        [StringLength(100, MinimumLength = 6,
            ErrorMessage = "Password must be at least 6 characters!")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
