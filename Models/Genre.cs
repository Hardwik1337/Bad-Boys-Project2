using System.ComponentModel.DataAnnotations;

namespace MovieHub.Models
{
    public class Genre
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва жанру обов'язкова")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Movie> Movies { get; set; } = new List<Movie>();
    }
}