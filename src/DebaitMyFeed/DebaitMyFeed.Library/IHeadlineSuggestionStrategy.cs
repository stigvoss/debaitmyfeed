namespace DebaitMyFeed.Library;

public interface IHeadlineSuggestionStrategy
{
    [Obsolete("Use SuggestHeadlineAsync(Article) instead.")]
    public Task<string?> SuggestHeadlineAsync(string articleText);
    
    public Task<string?> SuggestHeadlineAsync(Article article);
}