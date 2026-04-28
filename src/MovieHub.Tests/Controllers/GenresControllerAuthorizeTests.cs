using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using MovieHub.Controllers;
using Xunit;

namespace MovieHub.Tests.Controllers
{
    public class GenresControllerAuthorizeTests
    {
        [Fact]
        public void Controller_HasAuthorize_Admin()
        {
            var attr = typeof(GenresController).GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(attr);
            Assert.Equal("Admin", attr!.Roles);
        }
    }
}