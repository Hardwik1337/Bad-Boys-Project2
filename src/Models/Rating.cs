using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieHub.Models
{
    public class Rating
    {
        public int Id { get; set; }

        [Range(1, 10, ErrorMessage = "Оцінка має бути від 1 до 10")]
        [Display(Name = "Оцінка")]
        public int Score { get; set; }

        [Display(Name = "Фільм")]
        public int MovieId { get; set; }

        [NotMapped]
        public Movie? Movie { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}