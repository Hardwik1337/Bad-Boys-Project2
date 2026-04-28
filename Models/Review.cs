using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieHub.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Текст відгуку обов'язковий")]
        [Display(Name = "Текст відгуку")]
        public string Text { get; set; } = string.Empty;

        [Display(Name = "Дата створення")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Фільм")]
        public int MovieId { get; set; }

        [NotMapped]
        public Movie? Movie { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}