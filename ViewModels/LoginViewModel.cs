using System.ComponentModel.DataAnnotations;

namespace BusinessCRM.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter your email or username.")]
        [Display(Name = "Email or Username")]
        public string EmailOrUsername { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me on this device")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
