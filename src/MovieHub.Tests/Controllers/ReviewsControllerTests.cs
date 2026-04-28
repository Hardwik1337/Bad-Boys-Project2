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
    public class ReviewsControllerTests
    {
        [Fact]
        public async Task MyReviews_ReturnsOnlyCurrentUserReviews()
        {
            var appOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var catalogOptions = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            using var appContext = new ApplicationDbContext(appOptions);
            using var catalogContext = new CatalogDbContext(catalogOptions);

            var user = BuildUser("user-1", "u1");
            appContext.Users.Add(user);
            appContext.Reviews.Add(new Review
            {
                Id = 1,
                Text = "Mine",
                MovieId = 1,
                UserId = "user-1",
                CreatedAt = DateTime.Now
            });

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
            var controller = new ReviewsController(appContext, catalogContext, userManager.Object);
            controller.ControllerContext = BuildContext(user);

            var result = await controller.MyReviews();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Review>>(view.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Create_ValidReview_RedirectsToMyReviews()
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
            var controller = new ReviewsController(appContext, catalogContext, userManager.Object);
            controller.ControllerContext = BuildContext(user);

            var result = await controller.Create(new Review
            {
                Text = "Great",
                MovieId = 1,
                CreatedAt = DateTime.Now
            });

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("MyReviews", redirect.ActionName);
        }

        [Fact]
        public async Task Edit_Get_OtherUsersReview_RedirectsToAccessDenied()
        {
            var appOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var catalogOptions = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            using var appContext = new ApplicationDbContext(appOptions);
            using var catalogContext = new CatalogDbContext(catalogOptions);

            appContext.Reviews.Add(new Review { Id = 1, Text = "Other", MovieId = 1, UserId = "user-2" });
            await appContext.SaveChangesAsync();

            var user = BuildUser("user-1", "u1");
            var userManager = MockUserManager(user);
            var controller = new ReviewsController(appContext, catalogContext, userManager.Object);
            controller.ControllerContext = BuildContext(user);

            var result = await controller.Edit(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDenied", redirect.ActionName);
        }

        [Fact]
        public async Task Delete_Get_OtherUsersReview_ReturnsNotFound()
        {
            var appOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var catalogOptions = new DbContextOptionsBuilder<CatalogDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;

            using var appContext = new ApplicationDbContext(appOptions);
            using var catalogContext = new CatalogDbContext(catalogOptions);

            appContext.Reviews.Add(new Review { Id = 1, Text = "Other", MovieId = 1, UserId = "user-2" });
            await appContext.SaveChangesAsync();

            var user = BuildUser("user-1", "u1");
            var userManager = MockUserManager(user);
            var controller = new ReviewsController(appContext, catalogContext, userManager.Object);
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