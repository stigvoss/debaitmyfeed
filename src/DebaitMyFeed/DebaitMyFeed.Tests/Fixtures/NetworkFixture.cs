using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;

namespace DebaitMyFeed.Tests.Fixtures;

/// <summary>
/// The network fixture creates a shared network for all containers.
/// A fixture is used to be able to inject the network into other fixtures.
/// </summary>
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