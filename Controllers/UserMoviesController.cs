using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHub.Data;
using MovieHub.Models;

namespace MovieHub.Controllers
{
    [Authorize]
    public class UserMoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CatalogDbContext _catalogContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserMoviesController(
            ApplicationDbContext context,
            CatalogDbContext catalogContext,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _catalogContext = catalogContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var userMovies = await _context.UserMovies
                .Where(x => x.UserId == user.Id)
                .OrderByDescending(x => x.AddedAt)
                .ToListAsync();

            var movieIds = userMovies.Select(x => x.MovieId).Distinct().ToList();

            var movies = await _catalogContext.Movies
                .Include(m => m.Genre)
                .Where(m => movieIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m);

            var model = userMovies
                .Where(x => movies.ContainsKey(x.MovieId))
                .Select(x => new UserMovieListItemViewModel
                {
                    MovieId = x.MovieId,
                    AddedAt = x.AddedAt,
                    Title = movies[x.MovieId].Title,
                    Description = movies[x.MovieId].Description,
                    ReleaseYear = movies[x.MovieId].ReleaseYear,
                    GenreName = movies[x.MovieId].Genre?.Name ?? ""
                })
                .ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var movieExists = await _catalogContext.Movies.AnyAsync(m => m.Id == movieId);
            if (!movieExists) return NotFound();

            var alreadyExists = await _context.UserMovies
                .AnyAsync(x => x.UserId == user.Id && x.MovieId == movieId);

            if (!alreadyExists)
            {
                _context.UserMovies.Add(new UserMovie
                {
                    UserId = user.Id,
                    MovieId = movieId,
                    AddedAt = DateTime.Now
                });

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Фільм додано до моєї бази";
            }
            else
            {
                TempData["ErrorMessage"] = "Цей фільм уже є у твоїй базі";
            }

            return RedirectToAction("Index", "Movies");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var record = await _context.UserMovies
                .FirstOrDefaultAsync(x => x.UserId == user.Id && x.MovieId == movieId);

            if (record == null) return NotFound();

            _context.UserMovies.Remove(record);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Фільм видалено з моєї бази";
            return RedirectToAction(nameof(Index));
        }
    }

    public class UserMovieListItemViewModel
    {
        public int MovieId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public string GenreName { get; set; } = string.Empty;
        public DateTime AddedAt { get; set; }
    }
}