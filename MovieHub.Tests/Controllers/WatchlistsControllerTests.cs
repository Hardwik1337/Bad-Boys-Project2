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
    public class WatchlistsControllerTests
    {
        [Fact]
        public async Task Index_ReturnsOnlyCurrentUserWatchlists()
        {
            var appOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var catalogOptions = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            using var appContext = new ApplicationDbContext(appOptions);
            using var catalogContext = new CatalogDbContext(catalogOptions);

            appContext.Watchlists.AddRange(
                new Watchlist { Id = 1, Name = "Mine", UserId = "user-1" },
                new Watchlist { Id = 2, Name = "Other", UserId = "user-2" }
            );
            await appContext.SaveChangesAsync();

            var user = BuildUser("user-1", "watchuser");
            var userManager = MockUserManager(user);

            var controller = new WatchlistsController(appContext, catalogContext, userManager.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = BuildPrincipal(user) }
            };

            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Watchlist>>(view.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Create_ValidWatchlist_AssignsUserId_AndRedirectsToIndex()
        {
            var appOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var catalogOptions = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            using var appContext = new ApplicationDbContext(appOptions);
            using var catalogContext = new CatalogDbContext(catalogOptions);

            var user = BuildUser("user-1", "watchuser");
            var userManager = MockUserManager(user);

            var controller = new WatchlistsController(appContext, catalogContext, userManager.Object);
            var httpContext = new DefaultHttpContext { User = BuildPrincipal(user) };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var result = await controller.Create(new Watchlist { Name = "My List" });

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task AddMovie_NoWatchlists_RedirectsToCreate()
        {
            var appOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var catalogOptions = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            using var appContext = new ApplicationDbContext(appOptions);
            using var catalogContext = new CatalogDbContext(catalogOptions);

            catalogContext.Movies.Add(new Movie { Id = 1, Title = "Film", Description = "Desc", ReleaseYear = 2020, GenreId = 1 });
            await catalogContext.SaveChangesAsync();

            var user = BuildUser("user-1", "watchuser");
            var userManager = MockUserManager(user);

            var controller = new WatchlistsController(appContext, catalogContext, userManager.Object);
            var httpContext = new DefaultHttpContext { User = BuildPrincipal(user) };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var result = await controller.AddMovie(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Create", redirect.ActionName);
        }

        [Fact]
        public async Task RemoveMovie_OwnItem_RedirectsToDetails()
        {
            var appOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var catalogOptions = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            using var appContext = new ApplicationDbContext(appOptions);
            using var catalogContext = new CatalogDbContext(catalogOptions);

            appContext.Watchlists.Add(new Watchlist { Id = 1, Name = "Mine", UserId = "user-1" });
            appContext.WatchlistItems.Add(new WatchlistItem { Id = 1, WatchlistId = 1, MovieId = 1 });
            await appContext.SaveChangesAsync();

            var user = BuildUser("user-1", "watchuser");
            var userManager = MockUserManager(user);

            var controller = new WatchlistsController(appContext, catalogContext, userManager.Object);
            var httpContext = new DefaultHttpContext { User = BuildPrincipal(user) };
            controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var result = await controller.RemoveMovie(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
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