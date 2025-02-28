using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Testcontainers.Redis;

namespace DebaitMyFeed.Tests;

public class IntegrationTest : ContextTest, IClassFixture<RedisContainerFixture>
{
    private readonly RedisContainer redisContainer;
    private IContainer apiContainer;

    public IntegrationTest(RedisContainerFixture fixture)
    {
        this.redisContainer = fixture.Container;
    }
    
    [Fact]
    public async Task OpenAiTest()
    {
        Uri baseUri = new($"http://{this.apiContainer.Hostname}:{this.apiContainer.GetMappedPublicPort(8080)}");
        string endpointUrl = new Uri(baseUri, "/dr.dk/indland").ToString();
        
        IAPIResponse response = await Context.APIRequest.GetAsync(endpointUrl, new()
        {
            // The first run takes a while as nothing is cached in Redis all articles need to be processed by the LLM.
            Timeout = (int)TimeSpan.FromMinutes(2).TotalMilliseconds
        });
        
        Assert.Equal("application/rss+xml", response.Headers["content-type"]);
        Assert.Equal(200, response.Status);
    }
    
    [Fact]
    public async Task MistralAiTest()
    {
        Uri baseUri = new($"http://{this.apiContainer.Hostname}:{this.apiContainer.GetMappedPublicPort(8080)}");
        string endpointUrl = new Uri(baseUri, "/dr.dk/indland?provider=mistralai").ToString();
        
        IAPIResponse response = await Context.APIRequest.GetAsync(endpointUrl, new()
        {
            // The first run takes a while as nothing is cached in Redis all articles need to be processed by the LLM.
            Timeout = (int)TimeSpan.FromMinutes(2).TotalMilliseconds
        });
        
        Assert.Equal("application/rss+xml", response.Headers["content-type"]);
        Assert.Equal(200, response.Status);
    }

    public override async Task InitializeAsync()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        
        ApiKeyOptions options = new();
        configuration.GetSection("ApiKeys").Bind(options);
        
        IFutureDockerImage image = await BuildApiImage();
        this.apiContainer = await StartApiContainer(image.FullName, options);

        await base.InitializeAsync();
    }

    private async Task<IContainer> StartApiContainer(string imageName, ApiKeyOptions options)
    {
        var container = new ContainerBuilder()
            .WithImage(imageName)
            .WithEnvironment("Default__Strategy", "openai")
            .WithEnvironment("OpenAi__ApiKey", options.OpenAiApiKey)
            .WithEnvironment("MistralAi__ApiKey", options.MistralAiApiKey)
            .WithEnvironment("Redis__Configuration", this.redisContainer.GetConnectionString())
            .WithPortBinding(8080, true)
            .WithReuse(true)
            .Build();
        await container.StartAsync();
        return container;
    }

    private async Task<IFutureDockerImage> BuildApiImage()
    {
        DockerImage imageName = new("debaitmyfeed-api", tag: "integration-testing");
        IFutureDockerImage? image = new ImageFromDockerfileBuilder()
            .WithDockerfile("Dockerfile")
            .WithDockerfileDirectory("../../../../")
            .WithName(imageName)
            .WithCleanUp(true)
            .WithDeleteIfExists(true)
            .Build();
        await image.CreateAsync();
        
        if (image is null)
        {
            throw new InvalidOperationException("Failed to build the API image");
        }

        return image;
    }
}