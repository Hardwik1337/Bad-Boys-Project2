using System.ComponentModel.DataAnnotations;

namespace MovieHub.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва фільму обов'язкова")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Опис обов'язковий")]
        public string Description { get; set; } = string.Empty;

        [Range(1900, 2100, ErrorMessage = "Рік має бути в межах 1900-2100")]
        public int ReleaseYear { get; set; }

        [Display(Name = "Жанр")]
        public int GenreId { get; set; }

        public Genre? Genre { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}