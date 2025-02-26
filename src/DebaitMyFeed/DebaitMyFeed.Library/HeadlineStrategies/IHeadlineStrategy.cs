namespace DebaitMyFeed.Library.HeadlineStrategies;

public interface IHeadlineStrategy
{
    public string Id { get; }
    
    public byte MaxConcurrency { get; }
    
    public Task<string?> GetHeadlineAsync(FeedArticle article, CancellationToken cancellationToken);
}