using DebaitMyFeed.Library.HeadlineStrategies;

namespace DebaitMyFeed.Library.Debaiters;

public interface IFeedDebaiter
{
    public string Id { get; }
    
    public Task<ReadOnlyMemory<byte>> DebaitFeedAsync(
        IHeadlineStrategy strategy, 
        string? feedName = null);
}