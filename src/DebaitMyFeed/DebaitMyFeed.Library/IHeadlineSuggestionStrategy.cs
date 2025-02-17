namespace DebaitMyFeed.Library;

public interface IHeadlineSuggestionStrategy
{
    public string Id { get; }
    
    public Task<string?> SuggestHeadlineAsync(Article article);
}