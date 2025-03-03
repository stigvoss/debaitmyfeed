using Testcontainers.Redis;

namespace DebaitMyFeed.Tests.Fixtures;

public class RedisContainerFixture : IAsyncLifetime
{
    public const string NetworkAlias = "redis";
    public const int Port = 6379;
    
    private readonly NetworkFixture networkFixture;

    public RedisContainer? Container { get; set; }

    public RedisContainerFixture(NetworkFixture networkFixture)
    {
        this.networkFixture = networkFixture;
    }

    public async Task InitializeAsync()
    {
        Container = new RedisBuilder()
            .WithImage("redis:7.0")
            .WithName($"redis-{Guid.NewGuid():N}")
            .WithNetworkAliases(NetworkAlias)
            .WithNetwork(this.networkFixture.Network)
            .Build();
        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (Container != null)
        {
            await Container.StopAsync();
        }
    }
}