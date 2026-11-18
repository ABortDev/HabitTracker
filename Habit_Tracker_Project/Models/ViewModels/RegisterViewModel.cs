using System.ComponentModel.DataAnnotations;

namespace Habit_Tracker_Project.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[A-ZА-ЯІЇЄҐ][A-Za-zА-Яа-яІіЇїЄєҐґ'\- ]+$",
            ErrorMessage = "Start with a capital letter. Letters, spaces and hyphen only.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}