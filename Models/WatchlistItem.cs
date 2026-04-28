using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieHub.Models
{
    public class WatchlistItem
    {
        public int Id { get; set; }

        [Display(Name = "Список")]
        public int WatchlistId { get; set; }
        public Watchlist? Watchlist { get; set; }

        [Display(Name = "Фільм")]
        public int MovieId { get; set; }

        [NotMapped]
        public Movie? Movie { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.Now;
    }
}