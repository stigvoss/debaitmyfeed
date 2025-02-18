using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace DebaitMyFeed.Library.Debaiters.SonderborgNyt;

public class SonderborgNytDebaiter(
    IFusionCache cache,
    ILogger<FeedDebaiter> logger)
    : FeedDebaiter(cache, logger)
{
    public override string Id => "sonderborgnyt.dk";

    protected override Uri? GetFeedUrl(string? feedName)
    {
        return new Uri("https://sonderborgnyt.dk/feed/");
    }

    protected override async Task<Article> GetArticleAsync(string headline, DateTimeOffset published, Uri uri)
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "DebaitMyFeed/1.0");
        string result = await client.GetStringAsync(uri);

        IBrowsingContext context = BrowsingContext.New();
        IDocument document = await context.OpenAsync(req => req.Content(result));

        IElement? articleElement = document.QuerySelector(".post-text");
        
        if (articleElement is null)
        {
            throw new InvalidOperationException("Article element not found");
        }
        
        return new SonderborgNytArticle(headline, published, articleElement.TextContent);
    }
}