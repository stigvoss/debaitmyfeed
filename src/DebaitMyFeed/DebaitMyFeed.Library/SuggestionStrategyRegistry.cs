using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;

namespace DebaitMyFeed.Library;

public class SuggestionStrategyRegistry
{
    private readonly ImmutableDictionary<string, IHeadlineSuggestionStrategy> strategies;

    public SuggestionStrategyRegistry(IServiceProvider serviceProvider)
    {
        this.strategies = serviceProvider.GetServices<IHeadlineSuggestionStrategy>()
            .ToImmutableDictionary(strategy => strategy.Id, strategy => strategy);
    }
    
    public IHeadlineSuggestionStrategy GetStrategy(string id)
    {
        if (this.strategies.Count == 0)
        {
            throw new InvalidOperationException("No strategies found");
        }
        
        if (!this.strategies.TryGetValue(id, out IHeadlineSuggestionStrategy? strategy))
        {
            throw new InvalidOperationException($"No strategy found with id '{id}'");
        }

        return strategy;
    }
}