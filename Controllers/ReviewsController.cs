using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieHub.Data;
using MovieHub.Models;

namespace MovieHub.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CatalogDbContext _catalogContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewsController(
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
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .ToListAsync();

            var movieIds = reviews.Select(r => r.MovieId).Distinct().ToList();

            var movies = await _catalogContext.Movies
                .Where(m => movieIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m);

            foreach (var review in reviews)
            {
                if (movies.TryGetValue(review.MovieId, out var movie))
                {
                    review.Movie = movie;
                }
            }

            return View(reviews);
        }

        [Authorize]
        public async Task<IActionResult> MyReviews()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.UserId == user.Id)
                .ToListAsync();

            var movieIds = reviews.Select(r => r.MovieId).Distinct().ToList();

            var movies = await _catalogContext.Movies
                .Where(m => movieIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m);

            foreach (var review in reviews)
            {
                if (movies.TryGetValue(review.MovieId, out var movie))
                {
                    review.Movie = movie;
                }
            }

            return View("Index", reviews);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var review = await _context.Reviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null) return NotFound();

            review.Movie = await _catalogContext.Movies
                .FirstOrDefaultAsync(m => m.Id == review.MovieId);

            return View(review);
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
        public async Task<IActionResult> Create([Bind("Id,Text,CreatedAt,MovieId")] Review review)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            review.UserId = user.Id;

            if (!ModelState.IsValid)
            {
                var movies = await _catalogContext.Movies.ToListAsync();
                ViewData["MovieId"] = new SelectList(movies, "Id", "Title", review.MovieId);
                return View(review);
            }

            _context.Add(review);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(MyReviews));
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            if (review.UserId != user.Id)
                return RedirectToAction("AccessDenied", "Account");

            var movies = await _catalogContext.Movies.ToListAsync();
            ViewData["MovieId"] = new SelectList(movies, "Id", "Title", review.MovieId);
            return View(review);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Text,CreatedAt,MovieId")] Review review)
        {
            if (id != review.Id) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existingReview = await _context.Reviews
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingReview == null) return NotFound();

            if (existingReview.UserId != user.Id)
                return RedirectToAction("AccessDenied", "Account");

            review.UserId = user.Id;

            if (!ModelState.IsValid)
            {
                var movies = await _catalogContext.Movies.ToListAsync();
                ViewData["MovieId"] = new SelectList(movies, "Id", "Title", review.MovieId);
                return View(review);
            }

            _context.Update(review);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyReviews));
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var review = await _context.Reviews
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null) return NotFound();

            if (review.UserId != user.Id)
                return NotFound();

            review.Movie = await _catalogContext.Movies
                .FirstOrDefaultAsync(m => m.Id == review.MovieId);

            return View(review);
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            if (review.UserId != user.Id)
                return NotFound();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyReviews));
        }
    }
}