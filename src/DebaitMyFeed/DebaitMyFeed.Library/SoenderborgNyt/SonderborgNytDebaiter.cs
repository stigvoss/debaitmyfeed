using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DebaitMyFeed.Library.SoenderborgNyt;

public class SonderborgNytDebaiter(
    IHeadlineSuggestionStrategy suggestionStrategy,
    IMemoryCache cache,
    ILogger<FeedDebaiter> logger)
    : FeedDebaiter(suggestionStrategy, cache, logger)
{
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