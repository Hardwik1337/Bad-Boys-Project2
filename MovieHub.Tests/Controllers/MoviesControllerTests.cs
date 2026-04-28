using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieHub.Controllers;
using MovieHub.Data;
using MovieHub.Models;
using Xunit;

namespace MovieHub.Tests.Controllers
{
    public class MoviesControllerTests
    {
        [Fact]
        public async Task Index_ReturnsMoviesList()
        {
            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new CatalogDbContext(options);

            context.Genres.Add(new Genre { Id = 1, Name = "Sci-Fi" });
            context.Movies.Add(new Movie
            {
                Id = 1,
                Title = "Interstellar",
                Description = "Space",
                ReleaseYear = 2014,
                GenreId = 1
            });

            await context.SaveChangesAsync();

            var controller = new MoviesController(context);
            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Movie>>(view.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Details_ExistingId_ReturnsMovie()
        {
            var options = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var context = new CatalogDbContext(options);

            context.Genres.Add(new Genre { Id = 1, Name = "Action" });
            context.Movies.Add(new Movie
            {
                Id = 1,
                Title = "Spider-Man",
                Description = "Hero",
                ReleaseYear = 2002,
                GenreId = 1
            });

            await context.SaveChangesAsync();

            var controller = new MoviesController(context);
            var result = await controller.Details(1);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Movie>(view.Model);
            Assert.Equal("Spider-Man", model.Title);
        }
    }
}