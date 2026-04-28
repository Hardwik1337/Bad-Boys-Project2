using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using MovieHub.Controllers;
using Xunit;

namespace MovieHub.Tests.Controllers
{
    public class DashboardControllerAuthorizeTests
    {
        [Fact]
        public void Controller_HasAuthorize_AdminModerator()
        {
            var attr = typeof(DashboardController).GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(attr);
            Assert.Equal("Admin,Moderator", attr!.Roles);
        }
    }
}