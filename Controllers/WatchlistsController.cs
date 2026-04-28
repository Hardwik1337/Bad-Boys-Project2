using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MovieHub.Data;
using MovieHub.Models;

namespace MovieHub.Controllers
{
    [Authorize]
    public class WatchlistsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CatalogDbContext _catalogContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public WatchlistsController(
            ApplicationDbContext context,
            CatalogDbContext catalogContext,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _catalogContext = catalogContext;
            _userManager = userManager;
        }

        private async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(User);
        }

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Challenge();

            var watchlists = await _context.Watchlists
                .Where(w => w.UserId == user.Id)
                .Include(w => w.Items)
                .OrderBy(w => w.Name)
                .ToListAsync();

            var movieIds = watchlists
                .SelectMany(w => w.Items)
                .Select(i => i.MovieId)
                .Distinct()
                .ToList();

            var movies = await _catalogContext.Movies
                .Where(m => movieIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m);

            foreach (var watchlist in watchlists)
            {
                foreach (var item in watchlist.Items)
                {
                    if (movies.TryGetValue(item.MovieId, out var movie))
                    {
                        item.Movie = movie;
                    }
                }
            }

            return View(watchlists);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var user = await GetCurrentUserAsync();
            if (user == null) return Challenge();

            var watchlist = await _context.Watchlists
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == user.Id);

            if (watchlist == null) return NotFound();

            var movieIds = watchlist.Items.Select(i => i.MovieId).Distinct().ToList();

            var movies = await _catalogContext.Movies
                .Where(m => movieIds.Contains(m.Id))
                .ToDictionaryAsync(m => m.Id, m => m);

            foreach (var item in watchlist.Items)
            {
                if (movies.TryGetValue(item.MovieId, out var movie))
                {
                    item.Movie = movie;
                }
            }

            return View(watchlist);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Watchlist watchlist)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Challenge();

            watchlist.UserId = user.Id;
            watchlist.CreatedAt = DateTime.Now;

            if (!ModelState.IsValid)
                return View(watchlist);

            var exists = await _context.Watchlists
                .AnyAsync(w => w.UserId == user.Id && w.Name == watchlist.Name);

            if (exists)
            {
                ModelState.AddModelError(nameof(watchlist.Name), "Список з такою назвою вже існує");
                return View(watchlist);
            }

            _context.Watchlists.Add(watchlist);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Список створено";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await GetCurrentUserAsync();
            if (user == null) return Challenge();

            var watchlist = await _context.Watchlists
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == user.Id);

            if (watchlist == null) return NotFound();

            return View(watchlist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Watchlist watchlist)
        {
            if (id != watchlist.Id) return NotFound();

            var user = await GetCurrentUserAsync();
            if (user == null) return Challenge();

            var existing = await _context.Watchlists
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == user.Id);

            if (existing == null) return NotFound();

            if (!ModelState.IsValid)
                return View(watchlist);

            var duplicateName = await _context.Watchlists.AnyAsync(w =>
                w.UserId == user.Id &&
                w.Name == watchlist.Name &&
                w.Id != watchlist.Id);

            if (duplicateName)
            {
                ModelState.AddModelError(nameof(watchlist.Name), "Список з такою назвою вже існує");
                return View(watchlist);
            }

            existing.Name = watchlist.Name;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Список оновлено";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await GetCurrentUserAsync();
            if (user == null) return Challenge();

            var watchlist = await _context.Watchlists
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == user.Id);

            if (watchlist == null) return NotFound();

            return View(watchlist);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Challenge();

            var watchlist = await _context.Watchlists
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == user.Id);

            if (watchlist == null) return NotFound();

            _context.Watchlists.Remove(watchlist);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Список видалено";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> AddMovie(int movieId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Challenge();

            var movie = await _catalogContext.Movies.FirstOrDefaultAsync(m => m.Id == movieId);
            if (movie == null) return NotFound();

            var watchlists = await _context.Watchlists
                .Where(w => w.UserId == user.Id)
                .OrderBy(w => w.Name)
                .ToListAsync();

            if (!watchlists.Any())
            {
                TempData["ErrorMessage"] = "Спочатку створи хоча б один список";
                return RedirectToAction(nameof(Create));
            }

            ViewBag.Movie = movie;
            ViewBag.Watchlists = new SelectList(watchlists, "Id", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMovie(int movieId, int watchlistId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Challenge();

            var movie = await _catalogContext.Movies.FirstOrDefaultAsync(m => m.Id == movieId);
            if (movie == null) return NotFound();

            var watchlist = await _context.Watchlists
                .FirstOrDefaultAsync(w => w.Id == watchlistId && w.UserId == user.Id);

            if (watchlist == null)
                return RedirectToAction("AccessDenied", "Account");

            var exists = await _context.WatchlistItems
                .AnyAsync(i => i.WatchlistId == watchlistId && i.MovieId == movieId);

            if (exists)
            {
                TempData["ErrorMessage"] = "Цей фільм уже є в цьому списку";
                return RedirectToAction(nameof(Details), new { id = watchlistId });
            }

            var item = new WatchlistItem
            {
                WatchlistId = watchlistId,
                MovieId = movieId,
                AddedAt = DateTime.Now
            };

            _context.WatchlistItems.Add(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Фільм додано до списку";
            return RedirectToAction(nameof(Details), new { id = watchlistId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMovie(int itemId)
        {
            var user = await GetCurrentUserAsync();
            if (user == null) return Challenge();

            var item = await _context.WatchlistItems
                .Include(i => i.Watchlist)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null) return NotFound();

            if (item.Watchlist == null || item.Watchlist.UserId != user.Id)
                return RedirectToAction("AccessDenied", "Account");

            var watchlistId = item.WatchlistId;

            _context.WatchlistItems.Remove(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Фільм видалено зі списку";
            return RedirectToAction(nameof(Details), new { id = watchlistId });
        }
    }
}