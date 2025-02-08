namespace DebaitMyFeed.Library.JvDk;

public class JvArticle(string headline, DateTimeOffset published, string text, bool requiresSubscription)
    : Article(headline, published, text, requiresSubscription)
{
    public override string ArticleLanguage => "Danish";
    
    public override string Source => "JydskeVestkysten, jv.dk";
}