namespace DebaitMyFeed.Library;

public interface IFeedDebaiter
{
    public Task<ReadOnlyMemory<byte>> DebaitFeedAsync(string feedUrl);
}