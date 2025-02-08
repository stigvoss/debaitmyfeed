using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DebaitMyFeed.Library.DrDk;

public class DrFeedDebaiter(
    IHeadlineSuggestionStrategy suggestionStrategy, 
    IMemoryCache cache,
    ILogger<DrFeedDebaiter> logger)
    : FeedDebaiter(suggestionStrategy, cache, logger)
{
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