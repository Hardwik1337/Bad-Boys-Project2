using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MovieHub.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Ім'я обов'язкове")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ім'я має бути від 2 до 50 символів")]
        [Display(Name = "Повне ім'я")]
        public string FullName { get; set; } = string.Empty;
    }
}