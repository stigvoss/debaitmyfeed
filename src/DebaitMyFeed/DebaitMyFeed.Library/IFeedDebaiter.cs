namespace DebaitMyFeed.Library;

public interface IFeedDebaiter
{
    public string Id { get; }
    
    public Task<ReadOnlyMemory<byte>> DebaitFeedAsync(
        IHeadlineSuggestionStrategy suggestionStrategy, 
        string? feedName = null);
}