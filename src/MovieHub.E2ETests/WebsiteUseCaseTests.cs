using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Xunit;

namespace MovieHub.E2ETests;

public class WebsiteUseCaseTests : PageTest
{
    private const string BaseUrl = "https://moviehub-badboys-site-fnf9cdcafyeafud8.westeurope-01.azurewebsites.net";

    [Theory]
    [InlineData("/", "Home page opens")]
    [InlineData("/Home/Privacy", "Privacy page opens")]
    [InlineData("/Account/Login", "Login page opens")]
    [InlineData("/Account/Register", "Register page opens")]
    [InlineData("/Account/AccessDenied", "Access denied page opens")]
    [InlineData("/Movies", "Movies list opens")]
    [InlineData("/Movies/Create", "Movie create page opens")]
    [InlineData("/Movies/Edit/1", "Movie edit route works")]
    [InlineData("/MoviesBroken/Delete/1", "Movie delete route works")]
    [InlineData("/Genres", "Genres list opens")]
    [InlineData("/Genres/Create", "Genre create page opens")]
    [InlineData("/Genres/Details/1", "Genre details route works")]
    [InlineData("/Genres/Edit/1", "Genre edit route works")]
    [InlineData("/Genres/Delete/1", "Genre delete route works")]
    [InlineData("/Ratings", "Ratings list opens")]
    [InlineData("/Ratings/Create", "Rating create page opens")]
    [InlineData("/Ratings/Edit/1", "Rating edit route works")]
    [InlineData("/Ratings/Delete/1", "Rating delete route works")]
    [InlineData("/StreamingProviders", "Streaming providers list opens")]
    [InlineData("/StreamingProviders/Create", "Streaming provider create page opens")]
    [InlineData("/StreamingProviders/Details/1", "Provider details route works")]
    [InlineData("/StreamingProviders/Edit/1", "Provider edit route works")]
    [InlineData("/StreamingProviders/Delete/1", "Provider delete route works")]
    [InlineData("/Watchlists", "Watchlists list opens")]
    [InlineData("/Watchlists/Create", "Watchlist create page opens")]
    [InlineData("/Watchlists/Details/1", "Watchlist details route works")]
    [InlineData("/Watchlists/Edit/1", "Watchlist edit route works")]
    [InlineData("/UserMovies", "User movies page opens")]
    [InlineData("/Dashboard", "Dashboard opens or redirects")]
    [InlineData("/Profile", "Profile page opens or redirects")]
    public async Task Website_Route_Should_Not_Return_Server_Error(string path, string testName)
    {
        var response = await Page.GotoAsync(BaseUrl + path, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = 60000
        });

        Assert.NotNull(response);
        Assert.True(response.Status < 500, $"{testName}: server returned {response.Status}");

        var title = await Page.TitleAsync();
        Assert.False(string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(await Page.TextContentAsync("body")));
    }
}