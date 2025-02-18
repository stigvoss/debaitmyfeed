using System.Collections.Immutable;
using DebaitMyFeed.Library.HeadlineStrategies;
using Microsoft.Extensions.DependencyInjection;

namespace DebaitMyFeed.Library;

public class SuggestionStrategyRegistry
{
    private readonly ImmutableDictionary<string, IHeadlineStrategy> strategies;

    public SuggestionStrategyRegistry(IServiceProvider serviceProvider)
    {
        this.strategies = serviceProvider.GetServices<IHeadlineStrategy>()
            .ToImmutableDictionary(strategy => strategy.Id, strategy => strategy);
    }
    
    public IHeadlineStrategy GetStrategy(string id)
    {
        if (this.strategies.Count == 0)
        {
            throw new InvalidOperationException("No strategies found");
        }
        
        if (!this.strategies.TryGetValue(id, out IHeadlineStrategy? strategy))
        {
            throw new InvalidOperationException($"No strategy found with id '{id}'");
        }

        return strategy;
    }
}