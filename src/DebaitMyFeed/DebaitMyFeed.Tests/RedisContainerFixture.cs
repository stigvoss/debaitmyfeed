using Testcontainers.Redis;
using Testcontainers.Xunit;
using Xunit.Abstractions;

namespace DebaitMyFeed.Tests;

public sealed class RedisContainerFixture(IMessageSink messageSink)
    : ContainerFixture<RedisBuilder, RedisContainer>(messageSink)
{
    protected override RedisBuilder Configure(RedisBuilder builder)
    {
        return builder.WithImage("redis:7.0")
            .WithPortBinding(6379, true)
            .WithReuse(true);
    }
}