using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Microsoft.Extensions.Configuration;

namespace DebaitMyFeed.Tests.Fixtures;

public sealed class AppContainerFixture : IAsyncLifetime
{
    public IFutureDockerImage? Image { get; set; }
    public IContainer? Container { get; set; }
    
    private readonly NetworkFixture networkFixture;

    public AppContainerFixture(NetworkFixture networkFixture)
    {
        this.networkFixture = networkFixture;
    }

    public async Task InitializeAsync()
    {
        ApiKeyOptions options = new();
        new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build()
            .GetRequiredSection("ApiKeys")
            .Bind(options);
        
        Image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory("../../../../")
            .WithCleanUp(true)
            .WithDeleteIfExists(true)
            .Build();
        await Image.CreateAsync();
        
        Container = new ContainerBuilder()
            .WithName($"api-{Guid.NewGuid():N}")
            .WithImage(Image)
            .WithNetwork(this.networkFixture.Network)
            .WithEnvironment("Default__Strategy", "openai")
            .WithEnvironment("OpenAi__ApiKey", options.OpenAiApiKey)
            .WithEnvironment("MistralAi__ApiKey", options.MistralAiApiKey)
            .WithEnvironment("Redis__Configuration", $"{RedisContainerFixture.NetworkAlias}:{RedisContainerFixture.Port}")
            .WithPortBinding(8080, true)
            .Build();
        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (Container != null)
        {
            await Container.StopAsync();
        }
        
        if (Image != null)
        {
            await Image.DeleteAsync();
        }
    }
    
    class ApiKeyOptions
    {
        public string? OpenAiApiKey { get; set; }
    
        public string? MistralAiApiKey { get; set; }
    }
}