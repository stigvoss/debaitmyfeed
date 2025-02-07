namespace DebaitMyFeed.Library;

public interface IArticleTextExtractor
{
    public Task<string?> ExtractTextAsync(Uri uri);
}