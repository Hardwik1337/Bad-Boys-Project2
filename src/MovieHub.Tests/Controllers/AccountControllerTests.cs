using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using MovieHub.Controllers;
using MovieHub.Models;
using Xunit;

namespace MovieHub.Tests.Controllers
{
    public class AccountControllerTests
    {
        [Fact]
        public async Task Register_ValidModel_RedirectsToHome()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = MockUserManager(userStore.Object);
            var signInManager = MockSignInManager(userManager.Object);

            userManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);

            userManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser?)null);

            userManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            userManager.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            signInManager.Setup(x => x.SignInAsync(It.IsAny<ApplicationUser>(), false, null))
                .Returns(Task.CompletedTask);

            var controller = BuildController(userManager.Object, signInManager.Object);

            var model = new RegisterViewModel
            {
                FullName = "Test User",
                UserName = "testuser",
                Email = "test@test.com",
                Password = "123456",
                ConfirmPassword = "123456"
            };

            var result = await controller.Register(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal("Home", redirect.ControllerName);
        }

        [Fact]
        public async Task Login_InvalidModel_ReturnsView()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = MockUserManager(userStore.Object);
            var signInManager = MockSignInManager(userManager.Object);

            var controller = BuildController(userManager.Object, signInManager.Object);
            controller.ModelState.AddModelError("UserName", "Required");

            var model = new LoginViewModel
            {
                UserName = "",
                Password = ""
            };

            var result = await controller.Login(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.Equal(model, view.Model);
        }

        private static AccountController BuildController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            var controller = new AccountController(userManager, signInManager);
            var httpContext = new DefaultHttpContext();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>());

            return controller;
        }

        private static Mock<UserManager<ApplicationUser>> MockUserManager(IUserStore<ApplicationUser> store)
        {
            return new Mock<UserManager<ApplicationUser>>(
                store, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private static Mock<SignInManager<ApplicationUser>> MockSignInManager(UserManager<ApplicationUser> userManager)
        {
            return new Mock<SignInManager<ApplicationUser>>(
                userManager,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null!, null!, null!, null!);
        }
    }
}