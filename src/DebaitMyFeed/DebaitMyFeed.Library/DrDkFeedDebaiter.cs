using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Caching.Memory;

namespace DebaitMyFeed.Library;

public class DrDkFeedDebaiter : IFeedDebaiter
{
    private readonly IArticleTextExtractor extractor;
    private readonly IHeadlineSuggestionStrategy suggestionStrategy;
    private readonly IMemoryCache cache;

    public DrDkFeedDebaiter(
        IArticleTextExtractor extractor,
        IHeadlineSuggestionStrategy suggestionStrategy,
        IMemoryCache cache)
    {
        this.extractor = extractor;
        this.suggestionStrategy = suggestionStrategy;
        this.cache = cache;
    }
    
    public async Task<ReadOnlyMemory<byte>> DebaitFeedAsync(string feedUrl)
    {
        XmlReader reader = XmlReader.Create(feedUrl);
        SyndicationFeed feed = SyndicationFeed.Load(reader);

        await Parallel.ForEachAsync(feed.Items, async (item, _) =>
        {
            if (cache.TryGetValue(item.Id, out string? cachedHeadline))
            {
                item.Title = new TextSyndicationContent(cachedHeadline);
            }
            else
            {
                if (item.Links.FirstOrDefault()?.Uri is not Uri uri)
                {
                    return;
                }
                
                string? articleText = await extractor.ExtractTextAsync(uri);

                if (articleText is null)
                {
                    return;
                }
                
                string? headline = await suggestionStrategy.SuggestHeadlineAsync(articleText);

                cache.Set(item.Id, headline, TimeSpan.FromDays(7));

                item.Title = new TextSyndicationContent(headline);
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
}