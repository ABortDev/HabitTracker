using System.ComponentModel.DataAnnotations;

namespace Habit_Tracker_Project.Models.ViewModels
{
    public class ProfileViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[A-ZА-ЯІЇЄҐ][A-Za-zА-Яа-яІіЇїЄєҐґ'\- ]+$",
            ErrorMessage = "Start with a capital letter. Letters, spaces and hyphen only.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Phone]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string? Role { get; set; }
        public int TotalPoints { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsBlocked { get; set; }
    }
}