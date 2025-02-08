namespace DebaitMyFeed.Library.DrDk;

public class DrArticle(string headline, DateTimeOffset published, string text)
    : Article(headline, published, text, false)
{
    public override string ArticleLanguage => "Danish";
    
    public override string Source => "DR, dr.dk";
}