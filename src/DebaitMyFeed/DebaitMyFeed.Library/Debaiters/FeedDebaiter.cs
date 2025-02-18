using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using DebaitMyFeed.Library.HeadlineStrategies;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace DebaitMyFeed.Library.Debaiters;

public abstract class FeedDebaiter : IFeedDebaiter
{
    private readonly IFusionCache cache;
    private readonly ILogger<FeedDebaiter> logger;

    public FeedDebaiter(
        IFusionCache cache,
        ILogger<FeedDebaiter> logger)
    {
        this.cache = cache;
        this.logger = logger;
    }

    public abstract string Id { get; }

    protected abstract Uri? GetFeedUrl(string? feedName);

    public async Task<ReadOnlyMemory<byte>> DebaitFeedAsync(
        IHeadlineStrategy strategy, 
        string? feedName = null)
    {
        Uri? feedUri = GetFeedUrl(feedName);
        
        if (feedUri is null)
        {
            throw new InvalidOperationException("Invalid feed name");
        }
        
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "DebaitMyFeed/1.0");
        Stream feedXml = await client.GetStreamAsync(feedUri);
        
        XmlReader reader = XmlReader.Create(feedXml);
        SyndicationFeed feed = SyndicationFeed.Load(reader);

        await Parallel.ForEachAsync(
            feed.Items, 
            new ParallelOptions
            {
                MaxDegreeOfParallelism = strategy.MaxConcurrency
            }, 
            async (item, token) =>
        {
            string cacheKey = $"{strategy.Id}:{item.Id}";
            
            if (item.Links.FirstOrDefault()?.Uri is not Uri uri)
            {
                return;
            }

            try
            {
                string? headline = await this.cache.GetOrSetAsync<string?>(
                    cacheKey,
                    async (_, cancellationToken) =>
                    {
                        Article article = await GetArticleAsync(item.Title.Text, item.PublishDate, uri);

                        string? headline = (await strategy.GetHeadlineAsync(article, cancellationToken))?.TrimEnd('.');

                        // Indicate that the article requires a subscription in the headline.
                        if (article.RequiresSubscription)
                        {
                            headline = $"\ud83d\udd12 {headline}";
                        }
                    
                        this.logger.LogDebug("Debaited article {ArticleId} with headline {Headline} using {StrategyId}", item.Id, headline, strategy.Id);

                        return headline;
                    }, token: token);
            
                item.Title = new TextSyndicationContent(headline);
            } 
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to debait article {ArticleId}", item.Id);
            }
        });

        using MemoryStream stream = new MemoryStream();
        using XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings
        {
            Async = true,
            Encoding = Encoding.UTF8,
            Indent = true
        });
        
        feed.SaveAsRss20(writer);
        await writer.FlushAsync();

        return stream.ToArray();
    }

    protected abstract Task<Article> GetArticleAsync(string headline, DateTimeOffset published, Uri uri);
}