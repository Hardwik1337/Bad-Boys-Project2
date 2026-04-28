using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHub.Data;
using MovieHub.Models;

namespace MovieHub.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StreamingProvidersController : Controller
    {
        private readonly CatalogDbContext _context;

        public StreamingProvidersController(CatalogDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.StreamingProviders.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var provider = await _context.StreamingProviders.FirstOrDefaultAsync(p => p.Id == id);
            if (provider == null) return NotFound();

            return View(provider);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Url")] StreamingProvider provider)
        {
            if (!ModelState.IsValid)
                return View(provider);

            _context.Add(provider);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var provider = await _context.StreamingProviders.FindAsync(id);
            if (provider == null) return NotFound();

            return View(provider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Url")] StreamingProvider provider)
        {
            if (id != provider.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(provider);

            try
            {
                _context.Update(provider);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.StreamingProviders.Any(e => e.Id == provider.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var provider = await _context.StreamingProviders.FirstOrDefaultAsync(p => p.Id == id);
            if (provider == null) return NotFound();

            return View(provider);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var provider = await _context.StreamingProviders.FindAsync(id);
            if (provider != null)
            {
                _context.StreamingProviders.Remove(provider);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}