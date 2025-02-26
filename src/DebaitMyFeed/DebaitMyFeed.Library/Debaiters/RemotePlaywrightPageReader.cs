using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace DebaitMyFeed.Library.Debaiters;

public class RemotePlaywrightPageReader : IPageReader
{
    private readonly string endpoint;
    private static IPlaywright? playwrightInstance;
    
    public RemotePlaywrightPageReader(IOptions<RemotePlaywrightOptions> options)
    {
        this.endpoint = options.Value.Endpoint;
    }

    public async Task<string> ReadAsync(Uri url)
    {
        var playwright = await GetPlaywrightAsync();
        
        var browser = await playwright.Chromium.ConnectAsync(this.endpoint);
        var page = await browser.NewPageAsync();
        await page.GotoAsync(url.ToString());
        
        return await page.ContentAsync();
    }
    
    public async Task<IPlaywright> GetPlaywrightAsync()
    {
        if (playwrightInstance is null)
        {
            playwrightInstance = await Playwright.CreateAsync();
        }
        
        return playwrightInstance;
    }
}

public class RemotePlaywrightOptions
{
    public string Endpoint { get; set; } = "ws://localhost:3000";
}