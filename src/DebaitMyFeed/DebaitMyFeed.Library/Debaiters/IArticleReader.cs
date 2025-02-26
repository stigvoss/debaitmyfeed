using System.ServiceModel.Syndication;

namespace DebaitMyFeed.Library.Debaiters;

public interface IArticleReader
{
    Task<FeedArticle> ReadAsync(SyndicationItem syndicationItem, string html);
}