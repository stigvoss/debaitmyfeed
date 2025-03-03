using DebaitMyFeed.Tests.Fixtures;
using DotNet.Testcontainers.Containers;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Testcontainers.Redis;

namespace DebaitMyFeed.Tests;

[Collection("DockerCollection")]
public class IntegrationTest : ContextTest, IClassFixture<AppContainerFixture>, IClassFixture<RedisContainerFixture>
{
    private readonly RedisContainer? redis;
    private readonly IContainer? app;

    public IntegrationTest(AppContainerFixture appFixture, RedisContainerFixture redisFixture)
    {
        this.redis = redisFixture.Container;
        this.app = appFixture.Container;
    }
    
    [Fact]
    public async Task OpenAiTest()
    {
        Assert.NotNull(this.app);
        
        Uri baseUri = new($"http://{this.app.Hostname}:{this.app.GetMappedPublicPort(8080)}");
        string endpointUrl = new Uri(baseUri, "/dr.dk/indland").ToString();
        
        IAPIResponse response = await Context.APIRequest.GetAsync(endpointUrl, new()
        {
            // The first run takes a while as nothing is cached in Redis all articles need to be processed by the LLM.
            Timeout = (int)TimeSpan.FromMinutes(4).TotalMilliseconds
        });
        
        Assert.Equal(200, response.Status);
        Assert.True(response.Headers.ContainsKey("content-type"));
        Assert.Equal("application/rss+xml", response.Headers["content-type"]);
    }
    
    [Fact]
    public async Task MistralAiTest()
    {
        Assert.NotNull(this.app);
        
        Uri baseUri = new($"http://{this.app.Hostname}:{this.app.GetMappedPublicPort(8080)}");
        string endpointUrl = new Uri(baseUri, "/dr.dk/indland?provider=mistralai").ToString();
        
        IAPIResponse response = await Context.APIRequest.GetAsync(endpointUrl, new()
        {
            // The first run takes a while as nothing is cached in Redis all articles need to be processed by the LLM.
            Timeout = (int)TimeSpan.FromMinutes(4).TotalMilliseconds
        });
        
        Assert.Equal(200, response.Status);
        Assert.True(response.Headers.ContainsKey("content-type"));
        Assert.Equal("application/rss+xml", response.Headers["content-type"]);
    }
}