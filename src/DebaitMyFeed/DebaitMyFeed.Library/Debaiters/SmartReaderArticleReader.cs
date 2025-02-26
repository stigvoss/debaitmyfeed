using System.ServiceModel.Syndication;
using SmartReader;

namespace DebaitMyFeed.Library.Debaiters;

public class SmartReaderArticleReader : IArticleReader
{
    public async Task<FeedArticle> ReadAsync(SyndicationItem syndicationItem, string html)
    {
        string? url = syndicationItem.Links.FirstOrDefault()?.Uri.ToString();
        if (url is null)
        {
            throw new ArgumentException("SyndicationItem has no links");
        }
        
        var reader = new Reader(url, html)
        {
            MinScoreReaderable = 20
        };
        var readerArticle = await reader.GetArticleAsync();
        
        return new FeedArticle(
            readerArticle.Title,
            readerArticle.PublicationDate,
            readerArticle.Content,
            false,
            readerArticle.Language,
            readerArticle.SiteName);
    }
}