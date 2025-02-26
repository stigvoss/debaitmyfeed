namespace DebaitMyFeed.Library.Debaiters;

public interface IPageReader
{
    Task<string> ReadAsync(Uri url);
}