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

builder.Services.AddHttpClient("Scraper", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "DebaitMyFeed/1.0");
});

builder.AddOpenTelemetry();
builder.AddFusionCache();
builder.AddHeadlineStrategies();
builder.AddDebaiters();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/{sourceId}/{feedId}",
        async (
            [FromServices] DebaiterRegistry debaiterRegistry,
            [FromServices] HeadlineStrategyRegistry strategyRegistry,
            [FromServices] IOptions<HeadlineStrategyRegistryOptions> options,
            [FromRoute] string sourceId,
            [FromRoute] string feedId,
            [FromQuery(Name = "provider")] string? providerId = null) =>
        {
            if (string.IsNullOrWhiteSpace(providerId))
            {
                providerId = options.Value.Default;
            }
            
            try
            {
                IHeadlineStrategy strategy = strategyRegistry.GetStrategy(providerId);
                
                IFeedDebaiter debaiter = debaiterRegistry.GetDebaiter(sourceId);
                ReadOnlyMemory<byte> feed = await debaiter.DebaitFeedAsync(strategy, feedId);

                return Results.Bytes(feed, "application/rss+xml");
            } 
            catch (InvalidOperationException ex)
            {
                return Results.Json(new ProblemDetails
                {
                    Title = "The request could not be processed",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                },
                statusCode: StatusCodes.Status400BadRequest,
                contentType: "application/problem+json");
            }
        })
    .WithName("DebaitFeed")
    .WithOpenApi();

app.Run();