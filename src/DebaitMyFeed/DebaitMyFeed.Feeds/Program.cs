using DebaitMyFeed.Library;
using DebaitMyFeed.Library.DrDk;
using DebaitMyFeed.Library.JvDk;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddMemoryCache();
builder.Services.AddLogging(options => options.AddConsole());

builder.Services.AddOptions<OpenAiOptions>().Bind(builder.Configuration.GetSection("OpenAi"));
builder.Services.AddSingleton<IHeadlineSuggestionStrategy, OpenAiHeadlineSuggestionStrategy>();

builder.Services.AddKeyedSingleton<IFeedDebaiter, DrFeedDebaiter>("DrDk");
builder.Services.AddKeyedSingleton<IFeedDebaiter, JvFeedDebaiter>("JvDk");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/dr.dk/{feedName}",
        async (
            [FromKeyedServices("DrDk")]IFeedDebaiter debaiter,
            string feedName) =>
        {
            string[] validFeedNames =
            [
                "allenyheder",
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
            [FromKeyedServices("DrDk")]IFeedDebaiter debaiter,
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

app.MapGet("/jv.dk/{feedName}",
        async (
            [FromKeyedServices("JvDk")]IFeedDebaiter debaiter,
            string feedName) =>
        {
            string[] validFeedNames =
            [    
                "forside",
                "danmark",
                "erhverv",
                "sport",
                "esbjerg-fb",
                "soenderjyske",
                "kolding-if",
                "aabenraa",
                "billund",
                "esbjerg",
                "responsys",
                "haderslev",
                "kolding",
                "soenderborg",
                "toender",
                "varde",
                "vejen"
            ];
            
            if (!validFeedNames.Contains(feedName))
            {
                return Results.BadRequest("Invalid feed name");
            }

            string url = $"https://jv.dk/feed/{feedName}";
            
            ReadOnlyMemory<byte> feed = await debaiter.DebaitFeedAsync(url);

            return Results.Bytes(feed, "application/rss+xml");
        })
    .WithName("JvFeeds")
    .WithOpenApi();

app.Run();