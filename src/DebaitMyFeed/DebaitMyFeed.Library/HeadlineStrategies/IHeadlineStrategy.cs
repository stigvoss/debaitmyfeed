namespace DebaitMyFeed.Library;

public interface IHeadlineStrategy
{
    public string Id { get; }
    
    public byte MaxConcurrency { get; }
    
    public Task<string?> SuggestHeadlineAsync(Article article);
}