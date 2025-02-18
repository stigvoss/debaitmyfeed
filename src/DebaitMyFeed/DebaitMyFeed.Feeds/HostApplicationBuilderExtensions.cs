using DebaitMyFeed.Library;
using DebaitMyFeed.Library.Debaiters;
using DebaitMyFeed.Library.Debaiters.Dr;
using DebaitMyFeed.Library.Debaiters.Jv;
using DebaitMyFeed.Library.Debaiters.SonderborgNyt;
using DebaitMyFeed.Library.HeadlineStrategies;
using DebaitMyFeed.Library.HeadlineStrategies.MistralAi;
using DebaitMyFeed.Library.HeadlineStrategies.Ollama;
using DebaitMyFeed.Library.HeadlineStrategies.OpenAi;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using ZiggyCreatures.Caching.Fusion;
using IOpenTelemetryBuilder = OpenTelemetry.IOpenTelemetryBuilder;

namespace DebaitMyFeed.Feeds;

/// <summary>
/// Extensions for <see cref="IHostApplicationBuilder"/> to add services to the application.
/// </summary>
internal static class HostApplicationBuilderExtensions
{
    /// <summary>
    /// Add FusionCache to the application.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    internal static IHostApplicationBuilder AddFusionCache(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache();
        IFusionCacheBuilder fusionCacheBuilder = builder.Services
            .AddFusionCache()
            .WithRegisteredMemoryCache()
            .WithSystemTextJsonSerializer()
            .WithDefaultEntryOptions(options =>
            {
                options.SetDuration(TimeSpan.FromDays(1));
                options.SetDistributedCacheDuration(TimeSpan.FromDays(7));
            });

        if (builder.Configuration.GetSection("Redis").Exists())
        {
            builder.Services.AddStackExchangeRedisCache(options 
                => builder.Configuration.GetSection("Redis").Bind(options));

            fusionCacheBuilder.WithRegisteredDistributedCache();
        }

        return builder;
    }
    
    /// <summary>
    /// Add headline strategies to the application.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal static IHostApplicationBuilder AddHeadlineStrategies(this IHostApplicationBuilder builder)
    {
        if (builder.Configuration.GetSection("OpenAi").Exists())
        {
            builder.Services.AddOptions<OpenAiOptions>().Bind(builder.Configuration.GetSection("OpenAi"));
            builder.Services.AddSingleton<IHeadlineStrategy, OpenAiHeadlineStrategy>();
        }

        if (builder.Configuration.GetSection("MistralAi").Exists())
        {
            builder.Services.AddOptions<MistralAiOptions>().Bind(builder.Configuration.GetSection("MistralAi"));
            builder.Services.AddSingleton<IHeadlineStrategy, MistralAiHeadlineStrategy>();
        }

        if (builder.Configuration.GetSection("Ollama").Exists())
        {
            builder.Services.AddOptions<OllamaOptions>().Bind(builder.Configuration.GetSection("Ollama"));
            builder.Services.AddSingleton<IHeadlineStrategy, OllamaHeadlineStrategy>();
        }

        if (builder.Services.All(descriptor => descriptor.ServiceType != typeof(IHeadlineStrategy)))
        {
            throw new InvalidOperationException("No headline suggestion strategy found");
        }

        builder.Services.AddSingleton<HeadlineStrategyRegistry>();

        return builder;
    }
    
    /// <summary>
    /// Add OpenTelemetry to the application.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    internal static IHostApplicationBuilder AddOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        IOpenTelemetryBuilder otlpBuilder = builder.Services
            .AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                // Metrics provider from OpenTelemetry
                metrics.AddAspNetCoreInstrumentation();
                // Metrics provides by ASP.NET Core in .NET 8
                metrics.AddMeter("Microsoft.AspNetCore.Hosting");
                metrics.AddMeter("Microsoft.AspNetCore.Server.Kestrel");
            })
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddHttpClientInstrumentation();
            });

        string? otlpEndpoint = builder.Configuration.GetValue<string>("OTEL_EXPORTER_OTLP_ENDPOINT");
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            otlpBuilder.UseOtlpExporter();
        }

        return builder;
    }
    
    /// <summary>
    /// Add debaiters to the application.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    internal static IHostApplicationBuilder AddDebaiters(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IFeedDebaiter, DrFeedDebaiter>();
        builder.Services.AddSingleton<IFeedDebaiter, JvFeedDebaiter>();
        builder.Services.AddSingleton<IFeedDebaiter, SonderborgNytDebaiter>();
        builder.Services.AddSingleton<DebaiterRegistry>();
        
        return builder;
    }
}