using System.ComponentModel.DataAnnotations;

namespace Habit_Tracker_Project.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = LocalizationKeys.Validation.Required)]
        [EmailAddress(ErrorMessage = LocalizationKeys.Validation.InvalidEmail)]
        [DataType(DataType.EmailAddress)]
        [Display(Name = LocalizationKeys.Auth.Email)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = LocalizationKeys.Validation.Required)]
        [DataType(DataType.Password)]
        [Display(Name = LocalizationKeys.Auth.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = LocalizationKeys.Auth.RememberMe)]
        public bool RememberMe { get; set; }
    }
}