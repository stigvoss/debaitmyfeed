namespace DebaitMyFeed.Library.JvDk;

public class JvArticle(string headline, DateTimeOffset published, string text, bool requiresSubscription)
    : Article(headline, published, text, requiresSubscription)
{
    public override string ArticleLanguage => "Dansk";
    
    public override string Source => "JydskeVestkysten, jv.dk";
}