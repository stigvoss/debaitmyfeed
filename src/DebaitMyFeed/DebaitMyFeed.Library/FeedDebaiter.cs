using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace DebaitMyFeed.Library;

public abstract class FeedDebaiter : IFeedDebaiter
{
    private readonly IMemoryCache cache;
    private readonly ILogger<FeedDebaiter> logger;

    public FeedDebaiter(
        IMemoryCache cache,
        ILogger<FeedDebaiter> logger)
    {
        this.cache = cache;
        this.logger = logger;
    }

    public abstract string Id { get; }
    
    public abstract Uri? GetFeedUrl(string? feedName);

    public async Task<ReadOnlyMemory<byte>> DebaitFeedAsync(
        IHeadlineSuggestionStrategy suggestionStrategy, 
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
                MaxDegreeOfParallelism = suggestionStrategy.MaxConcurrency
            }, 
            async (item, _) =>
        {
            string cacheKey = $"{suggestionStrategy.Id}:{item.Id}";
            
            if (cache.TryGetValue(cacheKey, out string? cachedHeadline))
            {
                item.Title = new TextSyndicationContent(cachedHeadline);
            }
            else
            {
                if (item.Links.FirstOrDefault()?.Uri is not Uri uri)
                {
                    return;
                }

                try
                {
                    Article article = await GetArticleAsync(item.Title.Text, item.PublishDate, uri);

                    string? headline = await suggestionStrategy.SuggestHeadlineAsync(article);

                    // Indicate that the article requires a subscription in the headline.
                    if (article.RequiresSubscription)
                    {
                        headline = $"\ud83d\udcb6 {headline}";
                    }

                    cache.Set(cacheKey, headline, TimeSpan.FromDays(7));

                    item.Title = new TextSyndicationContent(headline);
                } 
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to debait article {ArticleId}", item.Id);
                }
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