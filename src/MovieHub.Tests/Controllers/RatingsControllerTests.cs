using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using MovieHub.Controllers;
using MovieHub.Data;
using MovieHub.Models;
using Xunit;

namespace MovieHub.Tests.Controllers
{
    public class RatingsControllerTests
    {
        [Fact]
        public async Task MyRatings_ReturnsOnlyCurrentUserRatings()
        {
            var appOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var catalogOptions = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            using var appContext = new ApplicationDbContext(appOptions);
            using var catalogContext = new CatalogDbContext(catalogOptions);

            var user = BuildUser("user-1", "u1");
            appContext.Users.Add(user);
            appContext.Ratings.Add(new Rating { Id = 1, Score = 8, MovieId = 1, UserId = "user-1" });

            catalogContext.Movies.Add(new Movie
            {
                Id = 1,
                Title = "Film",
                Description = "Desc",
                ReleaseYear = 2020,
                GenreId = 1
            });

            await appContext.SaveChangesAsync();
            await catalogContext.SaveChangesAsync();

            var userManager = MockUserManager(user);
            var controller = new RatingsController(appContext, catalogContext, userManager.Object);
            controller.ControllerContext = BuildContext(user);

            var result = await controller.MyRatings();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Rating>>(view.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Create_ValidRating_RedirectsToMyRatings()
        {
            var appOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var catalogOptions = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            using var appContext = new ApplicationDbContext(appOptions);
            using var catalogContext = new CatalogDbContext(catalogOptions);

            var user = BuildUser("user-1", "u1");
            catalogContext.Movies.Add(new Movie { Id = 1, Title = "Film", Description = "Desc", ReleaseYear = 2020, GenreId = 1 });
            await catalogContext.SaveChangesAsync();

            var userManager = MockUserManager(user);
            var controller = new RatingsController(appContext, catalogContext, userManager.Object);
            controller.ControllerContext = BuildContext(user);

            var result = await controller.Create(new Rating { Score = 9, MovieId = 1 });

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("MyRatings", redirect.ActionName);
        }

        [Fact]
        public async Task Edit_Get_OtherUsersRating_RedirectsToAccessDenied()
        {
            var appOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var catalogOptions = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            using var appContext = new ApplicationDbContext(appOptions);
            using var catalogContext = new CatalogDbContext(catalogOptions);

            appContext.Ratings.Add(new Rating { Id = 1, Score = 5, MovieId = 1, UserId = "user-2" });
            await appContext.SaveChangesAsync();

            var user = BuildUser("user-1", "u1");
            var userManager = MockUserManager(user);
            var controller = new RatingsController(appContext, catalogContext, userManager.Object);
            controller.ControllerContext = BuildContext(user);

            var result = await controller.Edit(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDenied", redirect.ActionName);
        }

        [Fact]
        public async Task Delete_Get_OtherUsersRating_ReturnsNotFound()
        {
            var appOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var catalogOptions = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            using var appContext = new ApplicationDbContext(appOptions);
            using var catalogContext = new CatalogDbContext(catalogOptions);

            appContext.Ratings.Add(new Rating { Id = 1, Score = 5, MovieId = 1, UserId = "user-2" });
            await appContext.SaveChangesAsync();

            var user = BuildUser("user-1", "u1");
            var userManager = MockUserManager(user);
            var controller = new RatingsController(appContext, catalogContext, userManager.Object);
            controller.ControllerContext = BuildContext(user);

            var result = await controller.Delete(1);

            Assert.IsType<NotFoundResult>(result);
        }

        private static ApplicationUser BuildUser(string id, string userName) =>
            new() { Id = id, UserName = userName, Email = $"{userName}@t.com", FullName = userName };

        private static ControllerContext BuildContext(ApplicationUser user) =>
            new()
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Name, user.UserName!)
                    }, "TestAuth"))
                }
            };

        private static Mock<UserManager<ApplicationUser>> MockUserManager(ApplicationUser user)
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var mock = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            mock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            return mock;
        }
    }
}