namespace DebaitMyFeed.Library;

public interface IHeadlineSuggestionStrategy
{
    public Task<string?> SuggestHeadlineAsync(Article article);
}