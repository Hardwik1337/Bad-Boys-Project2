using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using MovieHub.Controllers;
using Xunit;

namespace MovieHub.Tests.Controllers
{
    public class StreamingProvidersControllerAuthorizeTests
    {
        [Fact]
        public void Controller_HasAuthorize_Admin()
        {
            var attr = typeof(StreamingProvidersController).GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(attr);
            Assert.Equal("Admin", attr!.Roles);
        }
    }
}