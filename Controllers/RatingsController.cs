using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieHub.Data;
using MovieHub.Models;

namespace MovieHub.Controllers
{
    public class RatingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CatalogDbContext _catalogContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public RatingsController(
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
            var ratings = await _context.Ratings
                .Include(r => r.User)
                .ToListAsync();

            var movieIds = ratings.Select(r => r.MovieId).Distinct().ToList();

            var movies = await _catalogContext.Movies
                .Where(m => movieIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m);

            foreach (var rating in ratings)
            {
                if (movies.TryGetValue(rating.MovieId, out var movie))
                {
                    rating.Movie = movie;
                }
            }

            return View(ratings);
        }

        [Authorize]
        public async Task<IActionResult> MyRatings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var ratings = await _context.Ratings
                .Include(r => r.User)
                .Where(r => r.UserId == user.Id)
                .ToListAsync();

            var movieIds = ratings.Select(r => r.MovieId).Distinct().ToList();

            var movies = await _catalogContext.Movies
                .Where(m => movieIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m);

            foreach (var rating in ratings)
            {
                if (movies.TryGetValue(rating.MovieId, out var movie))
                {
                    rating.Movie = movie;
                }
            }

            return View("Index", ratings);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var rating = await _context.Ratings
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rating == null) return NotFound();

            rating.Movie = await _catalogContext.Movies
                .FirstOrDefaultAsync(m => m.Id == rating.MovieId);

            return View(rating);
        }

        [Authorize]
        public async Task<IActionResult> Create()
        {
            var movies = await _catalogContext.Movies.ToListAsync();
            ViewData["MovieId"] = new SelectList(movies, "Id", "Title");
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Score,MovieId")] Rating rating)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            rating.UserId = user.Id;

            if (!ModelState.IsValid)
            {
                var movies = await _catalogContext.Movies.ToListAsync();
                ViewData["MovieId"] = new SelectList(movies, "Id", "Title", rating.MovieId);
                return View(rating);
            }

            _context.Add(rating);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyRatings));
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var rating = await _context.Ratings.FindAsync(id);
            if (rating == null) return NotFound();

            if (rating.UserId != user.Id)
                return RedirectToAction("AccessDenied", "Account");

            var movies = await _catalogContext.Movies.ToListAsync();
            ViewData["MovieId"] = new SelectList(movies, "Id", "Title", rating.MovieId);
            return View(rating);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Score,MovieId")] Rating rating)
        {
            if (id != rating.Id) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existingRating = await _context.Ratings
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingRating == null) return NotFound();

            if (existingRating.UserId != user.Id)
                return RedirectToAction("AccessDenied", "Account");

            rating.UserId = user.Id;

            if (!ModelState.IsValid)
            {
                var movies = await _catalogContext.Movies.ToListAsync();
                ViewData["MovieId"] = new SelectList(movies, "Id", "Title", rating.MovieId);
                return View(rating);
            }

            _context.Update(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyRatings));
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var rating = await _context.Ratings
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rating == null) return NotFound();

            if (rating.UserId != user.Id)
                return NotFound();

            rating.Movie = await _catalogContext.Movies
                .FirstOrDefaultAsync(m => m.Id == rating.MovieId);

            return View(rating);
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var rating = await _context.Ratings.FindAsync(id);
            if (rating == null) return NotFound();

            if (rating.UserId != user.Id)
                return NotFound();

            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyRatings));
        }
    }
}