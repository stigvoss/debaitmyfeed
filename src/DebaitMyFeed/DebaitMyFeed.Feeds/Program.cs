using DebaitMyFeed.Library;
using DebaitMyFeed.Library.Debaiters;
using DebaitMyFeed.Library.Debaiters.Dr;
using DebaitMyFeed.Library.Debaiters.Jv;
using DebaitMyFeed.Library.Debaiters.SonderborgNyt;
using DebaitMyFeed.Library.HeadlineStrategies;
using DebaitMyFeed.Library.HeadlineStrategies.MistralAi;
using DebaitMyFeed.Library.HeadlineStrategies.Ollama;
using DebaitMyFeed.Library.HeadlineStrategies.OpenAi;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using ZiggyCreatures.Caching.Fusion;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddLogging(options => options.AddConsole());
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

var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
if (!string.IsNullOrWhiteSpace(otlpEndpoint))
{
    otlpBuilder.UseOtlpExporter();
}

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

if (builder.Services.All(descriptor => descriptor.ServiceType != typeof(IHeadlineStrategy)))
{
    throw new InvalidOperationException("No headline suggestion strategy found");
}

builder.Services.AddSingleton<IFeedDebaiter, DrFeedDebaiter>();
builder.Services.AddSingleton<IFeedDebaiter, JvFeedDebaiter>();
builder.Services.AddSingleton<IFeedDebaiter, SonderborgNytDebaiter>();

builder.Services.AddSingleton<SuggestionStrategyRegistry>();
builder.Services.AddSingleton<DebaiterRegistry>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/{feedId}/{feedName}",
        async (
            [FromServices] DebaiterRegistry debaiterRegistry,
            [FromServices] SuggestionStrategyRegistry suggestionStrategyRegistry,
            [FromRoute] string feedId,
            [FromRoute] string feedName,
            [FromQuery] string? provider = null) =>
        {
            if (string.IsNullOrWhiteSpace(provider))
            {
                provider = "openai";
            }
            
            try
            {
                IHeadlineStrategy strategy = suggestionStrategyRegistry.GetStrategy(provider);
                
                IFeedDebaiter debaiter = debaiterRegistry.GetDebaiter(feedId);
                ReadOnlyMemory<byte> feed = await debaiter.DebaitFeedAsync(strategy, feedName);

                return Results.Bytes(feed, "application/rss+xml");
            } 
            catch (InvalidOperationException e)
            {
                return Results.BadRequest(e.Message);
            }
        })
    .WithName("DebaitFeed")
    .WithOpenApi();

app.Run();