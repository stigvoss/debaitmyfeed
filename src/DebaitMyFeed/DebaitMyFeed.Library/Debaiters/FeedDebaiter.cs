using System.ServiceModel.Syndication;
using DebaitMyFeed.Library.HeadlineStrategies;

namespace DebaitMyFeed.Library.Debaiters;

public class FeedDebaiter
{
    private readonly IPageReader pageReader;
    private readonly IArticleReader articleReader;
    private readonly IHeadlineStrategy headlineStrategy;

    public FeedDebaiter(
        IPageReader pageReader, 
        IArticleReader articleReader)
    {
        this.pageReader = pageReader;
        this.articleReader = articleReader;
    }
    
    public async Task DebaitAsync(SyndicationFeed feed, CancellationToken cancellationToken)
    {
        foreach (var item in feed.Items)
        {
            var uri = item.Links.FirstOrDefault()?.Uri;

            var pageContent = await this.pageReader.ReadAsync(uri);
            var article = await this.articleReader.ReadAsync(item, pageContent);
            var headline = await this.headlineStrategy.GetHeadlineAsync(article, cancellationToken);
            
            item.Title = new TextSyndicationContent(headline);
        }
    }
}