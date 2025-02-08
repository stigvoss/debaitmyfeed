using AngleSharp.Dom;

namespace DebaitMyFeed.Library;

public abstract class Article
{
    public Article(string headline, DateTimeOffset published, string? text, bool requiresSubscription)
    {
        Headline = headline;
        Published = published;
        Text = text;
        RequiresSubscription = requiresSubscription;
    }
    
    public string Headline { get; }

    public DateTimeOffset Published { get; }

    public string? Text { get; }

    public bool RequiresSubscription { get; }
    
    public abstract string ArticleLanguage { get; }
    
    public abstract string Source { get; }
}