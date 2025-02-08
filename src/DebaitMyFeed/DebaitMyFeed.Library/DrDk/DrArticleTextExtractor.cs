using AngleSharp;
using AngleSharp.Dom;

namespace DebaitMyFeed.Library.DrDk;

public class DrArticleTextExtractor : IArticleTextExtractor
{
    private readonly HttpClient client;

    public DrArticleTextExtractor(HttpClient client)
    {
        this.client = client;
    }
    
    public async Task<string?> ExtractTextAsync(Uri uri)
    {
        string result = await this.client.GetStringAsync(uri);

        IBrowsingContext context = BrowsingContext.New();
        IDocument document = await context.OpenAsync(req => req.Content(result));
        
        IElement? articleElement = document.QuerySelector("article div[itemprop='articleBody']");

        return articleElement?.TextContent;
    }
}