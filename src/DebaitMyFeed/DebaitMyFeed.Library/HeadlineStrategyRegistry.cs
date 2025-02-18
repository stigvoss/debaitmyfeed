using System.Collections.Immutable;
using DebaitMyFeed.Library.HeadlineStrategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DebaitMyFeed.Library;

public class HeadlineStrategyRegistry
{
    private readonly ImmutableDictionary<string, IHeadlineStrategy> strategies;
    private readonly string? defaultStrategy;

    public HeadlineStrategyRegistry(
        IOptions<HeadlineStrategyRegistryOptions> options,
        IServiceProvider serviceProvider)
    {
        this.defaultStrategy = options.Value.Default;
        
        this.strategies = serviceProvider.GetServices<IHeadlineStrategy>()
            .ToImmutableDictionary(strategy => strategy.Id, strategy => strategy);
    }
    
    public IHeadlineStrategy GetStrategy(string? id)
    {
        if (this.strategies.Count == 0)
        {
            throw new InvalidOperationException("No strategies found");
        }

        if (string.IsNullOrWhiteSpace(id) && this.defaultStrategy is not null)
        {
            id = this.defaultStrategy;
        }

        if (string.IsNullOrWhiteSpace(id))
        {
            return this.strategies.Select(s => s.Value).First();
        }
        
        if (!this.strategies.TryGetValue(id, out IHeadlineStrategy? strategy))
        {
            throw new InvalidOperationException($"No strategy found with id '{id}'");
        }

        return strategy;
    }
}