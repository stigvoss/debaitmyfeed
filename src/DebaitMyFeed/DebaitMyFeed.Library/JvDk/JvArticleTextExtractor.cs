using AngleSharp;
using AngleSharp.Dom;

namespace DebaitMyFeed.Library.JvDk;

public class JvArticleTextExtractor : IArticleTextExtractor
{
    public const string PremiumArticle = "@@PREMIUM";
    
    private readonly HttpClient client;

    public JvArticleTextExtractor(HttpClient client)
    {
        this.client = client;
    }
    
    public async Task<string?> ExtractTextAsync(Uri uri)
    {
        string result = await this.client.GetStringAsync(uri);

        IBrowsingContext context = BrowsingContext.New();
        IDocument document = await context.OpenAsync(req => req.Content(result));
        
        IElement? premiumElement = document.QuerySelector("article header .plus");
        
        if (premiumElement is not null)
        {
            //return PremiumArticle;
        }
        
        IElement? articleResourceElement = document.QuerySelector("script#personalised-content-script");

        string? articleId = articleResourceElement?.GetAttribute("data-article-uuid");
        
        string articleResourceUrl = $"https://jv.dk/jfm-load-article-content/{articleId}?rss=&autologin-referral=";
        
        string articleResult = await this.client.GetStringAsync(articleResourceUrl);

        IBrowsingContext articleContext = BrowsingContext.New();
        IDocument articleDocument = await articleContext.OpenAsync(req => req.Content(articleResult));

        IElement? articleElement = articleDocument.QuerySelector(".article__parts");

        return articleElement?.TextContent;
    }
}