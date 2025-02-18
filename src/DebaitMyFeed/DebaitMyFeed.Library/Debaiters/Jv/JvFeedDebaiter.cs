using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DebaitMyFeed.Library.Debaiters.Jv;

public class JvFeedDebaiter(
    IMemoryCache cache,
    ILogger<JvFeedDebaiter> logger)
    : FeedDebaiter(cache, logger)
{
    public override string Id => "jv.dk";
    
    private readonly string[] validFeedNames =
    [
        "forside",
        "danmark",
        "erhverv",
        "sport",
        "esbjerg-fb",
        "soenderjyske",
        "kolding-if",
        "aabenraa",
        "billund",
        "esbjerg",
        "responsys",
        "haderslev",
        "kolding",
        "soenderborg",
        "toender",
        "varde",
        "vejen"
    ];

    protected override Uri? GetFeedUrl(string? feedName)
    {
        if (string.IsNullOrWhiteSpace(feedName) || !validFeedNames.Contains(feedName))
        {
            return null;
        }
        
        return new Uri($"https://jv.dk/feed/{feedName}");
    }

    protected override async Task<Article> GetArticleAsync(string headline, DateTimeOffset published, Uri uri)
    {
        HttpClient client = new HttpClient();
        string result = await client.GetStringAsync(uri);

        IBrowsingContext context = BrowsingContext.New();
        IDocument document = await context.OpenAsync(req => req.Content(result));
        
        IElement? premiumElement = document.QuerySelector("article header .plus");
        
        IElement? articleResourceElement = document.QuerySelector("script#personalised-content-script");

        string? articleId = articleResourceElement?.GetAttribute("data-article-uuid");
        
        string articleResourceUrl = $"https://jv.dk/jfm-load-article-content/{articleId}";
        
        string articleResult = await client.GetStringAsync(articleResourceUrl);

        IBrowsingContext articleContext = BrowsingContext.New();
        IDocument articleDocument = await articleContext.OpenAsync(req => req.Content(articleResult));

        IElement? articleElement = articleDocument.QuerySelector(".article__parts");
        
        if (articleElement is null)
        {
            throw new InvalidOperationException("Article element not found");
        }
        
        return new JvArticle(headline, published, articleElement.TextContent, premiumElement is not null);
    }
}