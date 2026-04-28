using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using MovieHub.Controllers;
using Xunit;

namespace MovieHub.Tests.Controllers
{
    public class MoviesControllerAuthorizeTests
    {
        [Fact]
        public void Create_Get_HasAuthorize_AdminModerator()
        {
            var method = typeof(MoviesController).GetMethod("Create", Type.EmptyTypes);
            var attr = method?.GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(attr);
            Assert.Equal("Admin,Moderator", attr!.Roles);
        }

        [Fact]
        public void Edit_Get_HasAuthorize_AdminModerator()
        {
            var method = typeof(MoviesController).GetMethod("Edit", new[] { typeof(int?) });
            var attr = method?.GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(attr);
            Assert.Equal("Admin,Moderator", attr!.Roles);
        }

        [Fact]
        public void Delete_Get_HasAuthorize_Admin()
        {
            var method = typeof(MoviesController).GetMethod("Delete");
            var attr = method?.GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(attr);
            Assert.Equal("Admin", attr!.Roles);
        }
    }
}