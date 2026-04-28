using System.ComponentModel.DataAnnotations;

namespace MovieHub.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ім'я користувача обов'язкове")]
        [Display(Name = "Ім'я користувача")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обов'язковий")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Запам'ятати мене")]
        public bool RememberMe { get; set; }
    }
}