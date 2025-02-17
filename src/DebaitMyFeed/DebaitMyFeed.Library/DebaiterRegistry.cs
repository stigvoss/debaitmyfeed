using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;

namespace DebaitMyFeed.Library;

public class DebaiterRegistry
{
    private readonly ImmutableDictionary<string ,IFeedDebaiter> debaiters;

    public DebaiterRegistry(IServiceProvider serviceProvider)
    {
        this.debaiters = serviceProvider.GetServices<IFeedDebaiter>()
            .ToImmutableDictionary(debaiter => debaiter.Id, debaiter => debaiter);
    }
    
    public IFeedDebaiter GetDebaiter(string id)
    {
        if (this.debaiters.Count == 0)
        {
            throw new InvalidOperationException("No debaiters found");
        }
        
        if (!this.debaiters.TryGetValue(id, out IFeedDebaiter? debaiter))
        {
            throw new InvalidOperationException($"No debaiter found with id '{id}'");
        }

        return debaiter;
    }
}