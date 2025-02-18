using DebaitMyFeed.Feeds;
using DebaitMyFeed.Library;
using DebaitMyFeed.Library.Debaiters;
using DebaitMyFeed.Library.HeadlineStrategies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddLogging(options => options.AddConsole());

builder.AddOpenTelemetry();
builder.AddFusionCache();
builder.AddHeadlineStrategies();
builder.AddDebaiters();

builder.Services.AddOptions<ApiOptions>()
    .Bind(builder.Configuration.GetSection("Api"));

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
            [FromServices] HeadlineStrategyRegistry suggestionStrategyRegistry,
            [FromServices] IOptions<ApiOptions> options,
            [FromRoute] string feedId,
            [FromRoute] string feedName,
            [FromQuery] string? provider = null) =>
        {
            if (string.IsNullOrWhiteSpace(provider))
            {
                provider = options.Value.DefaultStrategy;
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