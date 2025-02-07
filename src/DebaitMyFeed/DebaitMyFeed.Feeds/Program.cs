using System.ServiceModel.Syndication;
using System.Xml;
using DebaitMyFeed.Library;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddOptions<OpenAiOptions>().Bind(builder.Configuration.GetSection("OpenAi"));
builder.Services.AddSingleton<IArticleTextExtractor, DrArticleTextExtractor>();
builder.Services.AddSingleton<IHeadlineSuggestionStrategy, OpenAiHeadlineSuggestionStrategy>();
builder.Services.AddSingleton<IFeedDebaiter, DrDkFeedDebaiter>();
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/dr.dk/{feedName}",
        async (
            IFeedDebaiter debaiter,
            string feedName) =>
        {
            string[] validFeedNames =
            [
                "senestenyt",
                "indland",
                "udland",
                "penge",
                "politik",
                "sporten",
                "senestesport",
                "viden",
                "kultur",
                "musik",
                "vejret",
                "regionale"
            ];
            
            if (!validFeedNames.Contains(feedName))
            {
                return Results.BadRequest("Invalid feed name");
            }

            string url = $"https://www.dr.dk/nyheder/service/feeds/{feedName}";
            
            ReadOnlyMemory<byte> feed = await debaiter.DebaitFeedAsync(url);

            return Results.Bytes(feed, "application/rss+xml");
        })
    .WithName("DrFeeds")
    .WithOpenApi();



app.MapGet("/dr.dk/regionale/{feedName}",
        async (
            IFeedDebaiter debaiter,
            string feedName) =>
        {
            string[] validFeedNames =
            [
                "kbh",
                "bornholm",
                "syd",
                "fyn",
                "vest",
                "nord",
                "trekanten",
                "sjaelland",
                "oestjylland"
            ];
            
            if (!validFeedNames.Contains(feedName))
            {
                return Results.BadRequest("Invalid feed name");
            }

            string url = $"https://www.dr.dk/nyheder/service/feeds/regionale/{feedName}";
            
            ReadOnlyMemory<byte> feed = await debaiter.DebaitFeedAsync(url);

            return Results.Bytes(feed, "application/rss+xml");
        })
    .WithName("DrRegionalFeeds")
    .WithOpenApi();

app.Run();