namespace DebaitMyFeed.Library.SonderborgNyt;

public class SonderborgNytArticle(string headline, DateTimeOffset published, string? text)
    : Article(headline, published, text, false)
{
    public override string ArticleLanguage => "Danish";
    public override string Source => "Sønderborg Nyt, sonderborgnyt.dk";
}