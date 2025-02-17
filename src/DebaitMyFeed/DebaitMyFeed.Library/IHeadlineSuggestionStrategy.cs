namespace DebaitMyFeed.Library;

public interface IHeadlineSuggestionStrategy
{
    public byte MaxConcurrency { get; }
    
    public Task<string?> SuggestHeadlineAsync(Article article);
}