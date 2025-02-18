using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace DebaitMyFeed.Library.Debaiters.Dr;

public class DrFeedDebaiter(
    IFusionCache cache,
    ILogger<DrFeedDebaiter> logger)
    : FeedDebaiter(cache, logger)
{
    public override string Id => "dr.dk";
    
    /// <summary>
    /// Valid feed names for DR.dk, with a boolean indicating whether the feed is regional.
    /// </summary>
    private readonly Dictionary<string, bool> validFeedNames = new()
    {
        { "allenyheder", false },
        { "senestenyt", false },
        { "indland", false },
        { "udland", false },
        { "penge", false },
        { "politik", false },
        { "sporten", false },
        { "senestesport", false },
        { "viden", false },
        { "kultur", false },
        { "musik", false },
        { "vejret", false },
        { "kbh", true },
        { "bornholm", true },
        { "syd", true },
        { "fyn", true },
        { "vest", true },
        { "nord", true },
        { "trekanten", true },
        { "sjaelland", true },
        { "oestjylland", true }
    };

    protected override Uri? GetFeedUrl(string? feedName)
    {
        if (string.IsNullOrWhiteSpace(feedName) || !this.validFeedNames.TryGetValue(feedName, out var isRegional))
        {
            return null;
        }
        
        if (isRegional)
        {
            return new Uri($"https://www.dr.dk/nyheder/service/feeds/regionale/{feedName}");
        }
        
        return new Uri($"https://www.dr.dk/nyheder/service/feeds/{feedName}");
    }

    protected override async Task<Article> GetArticleAsync(string headline, DateTimeOffset published, Uri uri)
    {
        HttpClient client = new HttpClient();
        string result = await client.GetStringAsync(uri);
        
        IBrowsingContext context = BrowsingContext.New();
        IDocument document = await context.OpenAsync(req => req.Content(result));
        
        IElement? articleElement = document.QuerySelector("article div[itemprop='articleBody']");
        
        if (articleElement is null)
        {
            throw new InvalidOperationException("Article element not found");
        }

        return new DrArticle(headline, published, articleElement.TextContent);
    }
}