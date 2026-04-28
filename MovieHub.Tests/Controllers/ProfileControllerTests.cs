using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieHub.Controllers;
using MovieHub.Models;
using Xunit;

namespace MovieHub.Tests.Controllers
{
    public class ProfileControllerTests
    {
        [Fact]
        public async Task Index_ReturnsView_WithCurrentUser()
        {
            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "profileuser",
                Email = "profile@test.com",
                FullName = "Profile User"
            };

            var userManager = MockUserManager();
            userManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            userManager.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });

            var controller = new ProfileController(userManager.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = BuildPrincipal(user)
                }
            };

            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ApplicationUser>(view.Model);
            Assert.Equal("profileuser", model.UserName);
        }

        private static ClaimsPrincipal BuildPrincipal(ApplicationUser user)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!)
            }, "TestAuth"));
        }

        private static Mock<UserManager<ApplicationUser>> MockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }
    }
}