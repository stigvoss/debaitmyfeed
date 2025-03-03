using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;

namespace DebaitMyFeed.Tests.Fixtures;

public class NetworkFixture : IAsyncLifetime
{
    public INetwork? Network { get; private set; }

    public async Task InitializeAsync()
    {
        // Create a shared network for all containers
        Network = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("D"))
            .WithCleanUp(true)
            .Build();
        await Network.CreateAsync();
    }

    public async Task DisposeAsync()
    {
        if (Network != null)
        {
            await Network.DeleteAsync();
        }
    }
}