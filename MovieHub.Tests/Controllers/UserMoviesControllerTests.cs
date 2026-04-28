using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using MovieHub.Controllers;
using MovieHub.Data;
using MovieHub.Models;
using Xunit;

namespace MovieHub.Tests.Controllers
{
    public class UserMoviesControllerTests
    {
        [Fact]
        public async Task Index_ReturnsCurrentUsersMovies()
        {
            var appOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var catalogOptions = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            using var appContext = new ApplicationDbContext(appOptions);
            using var catalogContext = new CatalogDbContext(catalogOptions);

            appContext.UserMovies.Add(new UserMovie { Id = 1, UserId = "user-1", MovieId = 1, AddedAt = DateTime.Now });
            catalogContext.Genres.Add(new Genre { Id = 1, Name = "Action" });
            catalogContext.Movies.Add(new Movie { Id = 1, Title = "Spider-Man", Description = "Hero", ReleaseYear = 2002, GenreId = 1 });

            await appContext.SaveChangesAsync();
            await catalogContext.SaveChangesAsync();

            var user = BuildUser("user-1", "u1");
            var userManager = MockUserManager(user);

            var controller = new UserMoviesController(appContext, catalogContext, userManager.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = BuildPrincipal(user) }
            };

            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            Assert.NotNull(view.Model);
        }

        [Fact]
        public async Task Add_NewMovie_RedirectsToMoviesIndex()
        {
            var appOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var catalogOptions = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            using var appContext = new ApplicationDbContext(appOptions);
            using var catalogContext = new CatalogDbContext(catalogOptions);

            catalogContext.Genres.Add(new Genre { Id = 1, Name = "Action" });
            catalogContext.Movies.Add(new Movie { Id = 1, Title = "Spider-Man", Description = "Hero", ReleaseYear = 2002, GenreId = 1 });
            await catalogContext.SaveChangesAsync();

            var user = BuildUser("user-1", "u1");
            var userManager = MockUserManager(user);

            var controller = new UserMoviesController(appContext, catalogContext, userManager.Object);
            var httpContext = new DefaultHttpContext { User = BuildPrincipal(user) };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var result = await controller.Add(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Movies", redirect.ControllerName);
        }

        [Fact]
        public async Task Remove_ExistingMovie_RedirectsToIndex()
        {
            var appOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var catalogOptions = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            using var appContext = new ApplicationDbContext(appOptions);
            using var catalogContext = new CatalogDbContext(catalogOptions);

            appContext.UserMovies.Add(new UserMovie { Id = 1, UserId = "user-1", MovieId = 1, AddedAt = DateTime.Now });
            await appContext.SaveChangesAsync();

            var user = BuildUser("user-1", "u1");
            var userManager = MockUserManager(user);

            var controller = new UserMoviesController(appContext, catalogContext, userManager.Object);
            var httpContext = new DefaultHttpContext { User = BuildPrincipal(user) };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var result = await controller.Remove(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        private static ApplicationUser BuildUser(string id, string userName) =>
            new() { Id = id, UserName = userName, Email = $"{userName}@t.com", FullName = userName };

        private static ClaimsPrincipal BuildPrincipal(ApplicationUser user) =>
            new(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!)
            }, "TestAuth"));

        private static Mock<UserManager<ApplicationUser>> MockUserManager(ApplicationUser user)
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mock = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            mock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            return mock;
        }
    }
}