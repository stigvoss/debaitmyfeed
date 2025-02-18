using DebaitMyFeed.Library.HeadlineStrategies;

namespace DebaitMyFeed.Library.Debaiters;

/// <summary>
/// A debaiter is a service which removes clickbait from a feed.
/// </summary>
public interface IFeedDebaiter
{
    /// <summary>
    /// The unique identifier of the debaiter.
    /// </summary>
    public string Id { get; }
    
    /// <summary>
    /// Debait <see cref="Article.Headline"/>s in a feed of <see cref="Article"/>s using the specified <see cref="IHeadlineStrategy"/>.
    /// </summary>
    /// <param name="strategy">The strategy to use for debaiting headlines.</param>
    /// <param name="feedName"></param>
    /// <returns></returns>
    public Task<ReadOnlyMemory<byte>> DebaitFeedAsync(
        IHeadlineStrategy strategy, 
        string? feedName = null);
}