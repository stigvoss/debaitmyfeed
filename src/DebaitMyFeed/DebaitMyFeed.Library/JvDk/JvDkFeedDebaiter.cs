using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace DebaitMyFeed.Library.JvDk;

public class JvDkFeedDebaiter : IFeedDebaiter
{
    private readonly IArticleTextExtractor extractor;
    private readonly IHeadlineSuggestionStrategy suggestionStrategy;
    private readonly IMemoryCache cache;

    public JvDkFeedDebaiter(
        [FromKeyedServices("JvDk")]IArticleTextExtractor extractor,
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

                string? headline;
                if (articleText == JvArticleTextExtractor.PremiumArticle)
                {
                    headline = $"[P] {item.Title.Text}";
                }
                else
                {
                    headline = await suggestionStrategy.SuggestHeadlineAsync(articleText);
                }

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