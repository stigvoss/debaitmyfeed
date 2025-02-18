namespace DebaitMyFeed.Library.Debaiters.SonderborgNyt;

public class SonderborgNytArticle(string headline, DateTimeOffset published, string? text)
    : Article(headline, published, text, false)
{
    public override string ArticleLanguage => "Dansk";
    public override string Source => "Sønderborg Nyt, sonderborgnyt.dk";
}