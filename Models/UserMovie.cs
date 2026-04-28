using System.ComponentModel.DataAnnotations;

namespace MovieHub.Models
{
    public class UserMovie
    {
        public int Id { get; set; }

        [Display(Name = "ID фільму з каталогу")]
        public int MovieId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        [Display(Name = "Дата додавання")]
        public DateTime AddedAt { get; set; } = DateTime.Now;
    }
}