using System.ComponentModel.DataAnnotations;

namespace MovieHub.Models
{
    public class Watchlist
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Назва списку обов'язкова")]
        [StringLength(100, ErrorMessage = "Максимум 100 символів")]
        [Display(Name = "Назва списку")]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public ICollection<WatchlistItem> Items { get; set; } = new List<WatchlistItem>();
    }
}