using DebaitMyFeed.Feeds;
using DebaitMyFeed.Library;
using DebaitMyFeed.Library.Debaiters;
using DebaitMyFeed.Library.HeadlineStrategies;
using DebaitMyFeed.Library.HeadlineStrategies.OpenAi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddLogging(options => options.AddConsole());

builder.Services.AddHttpClient("Scraper", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "DebaitMyFeed/1.0");
});

builder.Services.AddSingleton<IPageReader, RemotePlaywrightPageReader>();
builder.Services.AddSingleton<IArticleReader, SmartReaderArticleReader>();

builder.AddOpenTelemetry();
builder.AddFusionCache();
builder.AddHeadlineStrategies();
//builder.AddDebaiters();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.MapGet("/{sourceId}/{feedId}",
//         async (
//             [FromServices] HeadlineStrategyRegistry strategyRegistry,
//             [FromServices] IOptions<HeadlineStrategyRegistryOptions> options,
//             [FromRoute] string sourceId,
//             [FromRoute] string feedId,
//             [FromQuery(Name = "provider")] string? providerId = null) =>
//         {
//             if (string.IsNullOrWhiteSpace(providerId))
//             {
//                 providerId = options.Value.Default;
//             }
//             
//             try
//             {
//                 IHeadlineStrategy strategy = strategyRegistry.GetStrategy(providerId);
//                 
//                 IFeedDebaiter debaiter = debaiterRegistry.GetDebaiter(sourceId);
//                 ReadOnlyMemory<byte> feed = await debaiter.DebaitFeedAsync(strategy, feedId);
//
//                 return Results.Bytes(feed, "application/rss+xml");
//             } 
//             catch (InvalidOperationException ex)
//             {
//                 return Results.Problem(
//                     title: "The request could not be processed",
//                     detail: ex.Message,
//                     statusCode: StatusCodes.Status400BadRequest);
//             }
//         })
//     .WithName("DebaitFeed")
//     .WithOpenApi();

app.MapGet("/",
        async (
            [FromServices] FeedDebaiter debaiter,
            [FromServices] HeadlineStrategyRegistry strategyRegistry,
            [FromServices] IOptions<HeadlineStrategyRegistryOptions> strategyOptions,
            [FromServices] IHttpClientFactory clientFactory,
            [FromQuery(Name = "f")] Uri feedUri,
            [FromQuery(Name = "s")] string? strategyId = null) =>
        {
            if (string.IsNullOrWhiteSpace(strategyId))
            {
                strategyId = strategyOptions.Value.Default;
            }
            
            IHeadlineStrategy strategy = strategyRegistry.GetStrategy(strategyId);
            
            debaiter.DebaitAsync(item)
            
            return Results.Bytes(feedBytes, "application/rss+xml");
        })
    .WithName("GenericFeed")
    .WithOpenApi();

app.Run();