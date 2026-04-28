using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHub.Data;
using MovieHub.Models;

namespace MovieHub.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _appContext;
        private readonly CatalogDbContext _catalogContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            ApplicationDbContext appContext,
            CatalogDbContext catalogContext,
            UserManager<ApplicationUser> userManager)
        {
            _appContext = appContext;
            _catalogContext = catalogContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            var model = new DashboardViewModel
            {
                MoviesCount = await _catalogContext.Movies.CountAsync(),
                GenresCount = await _catalogContext.Genres.CountAsync(),
                ReviewsCount = await _appContext.Reviews.CountAsync(),
                RatingsCount = await _appContext.Ratings.CountAsync(),
                StreamingProvidersCount = await _catalogContext.StreamingProviders.CountAsync(),
                WatchlistsCount = await _appContext.Watchlists.CountAsync(),
                UsersCount = users.Count,

                LatestMovies = await _catalogContext.Movies
                    .Include(m => m.Genre)
                    .OrderByDescending(m => m.Id)
                    .Take(5)
                    .ToListAsync(),

                LatestReviews = await _appContext.Reviews
                    .OrderByDescending(r => r.Id)
                    .Take(5)
                    .ToListAsync(),

                LatestUsers = users
                    .OrderByDescending(u => u.Id)
                    .Take(5)
                    .ToList()
            };

            return View(model);
        }
    }

    public class DashboardViewModel
    {
        public int MoviesCount { get; set; }
        public int GenresCount { get; set; }
        public int ReviewsCount { get; set; }
        public int RatingsCount { get; set; }
        public int StreamingProvidersCount { get; set; }
        public int WatchlistsCount { get; set; }
        public int UsersCount { get; set; }

        public List<Movie> LatestMovies { get; set; } = new();
        public List<Review> LatestReviews { get; set; } = new();
        public List<ApplicationUser> LatestUsers { get; set; } = new();
    }
}