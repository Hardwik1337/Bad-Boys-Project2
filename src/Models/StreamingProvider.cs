using System.ComponentModel.DataAnnotations;

namespace MovieHub.Models
{
    public class StreamingProvider
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва сервісу обов'язкова")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Посилання обов'язкове")]
        [Url(ErrorMessage = "Некоректне посилання")]
        public string Url { get; set; } = string.Empty;
    }
}