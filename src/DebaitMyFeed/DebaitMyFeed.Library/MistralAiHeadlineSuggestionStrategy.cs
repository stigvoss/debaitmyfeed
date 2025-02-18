using Microsoft.Extensions.Options;
using Mistral.SDK;
using Mistral.SDK.DTOs;

namespace DebaitMyFeed.Library;

public class MistralAiOptions
{
    public required string ApiKey { get; set; }
    
    public string Model { get; set; } = ModelDefinitions.MistralLarge;
}

public class MistralAiHeadlineSuggestionStrategy : IHeadlineSuggestionStrategy
{
    private readonly MistralClient client;
    private readonly string model;

    public MistralAiHeadlineSuggestionStrategy(IOptions<MistralAiOptions> options)
    {
        MistralAiOptions mistralAiOptions = options.Value;
        
        this.model = mistralAiOptions.Model;
        
        APIAuthentication authentication = new APIAuthentication(mistralAiOptions.ApiKey);
        this.client = new MistralClient(authentication);
    }

    public string Id => "mistralai";
    
    public byte MaxConcurrency => 3;

    public async Task<string?> SuggestHeadlineAsync(Article article)
    {
        string dateContext =
            $"""
                Nuværende dato og klokkeslæt er {DateTimeOffset.Now:o}.
            """;
        
        string instructionsPrompt = 
            """
                Brugeren vil give dig artiklens tekst.
                Du vil bruge teksten til at generere en klar, præcis, specifik og beskrivende overskrift til artiklen som er fri for clickbait.
                Fokusér på nøgle pointerne i artiklen.
                Tonen i overskriften skal være neutral.
                Hvis nøglepersonerne i artiklen forventes at være kendt af læseren, overvej at inkludere deres navn,
                eller hvis nøglepersoner har en titel der er betydende for artiklen, overvej at inkludere titlen.
                Hvis det er mere sandsynligt at læseren vil genkende personens titel end navn, så overvej at bruge titlen i stedet for navnet.
                Hvis personen bag titlen er meget kendt, f.eks. præsident af USA, så brug titlen frem for navnet.
                Hvis artiklen indhold omhandler nogle bestemte nøglesteder, overvej at inkludere stedet i overskriften.
                Hvis artiklen nævner alment kendte organisationer eller virksomheder, overvej at inkludere navnet i overskriften, så længe det er relevant for artiklens kerneindhold. 
                Brug artiklens original sprog når du laver overskriften.
                Du vil kun give overskriften, intet andet i dit svar, ingen forklaringer eller noter. Kun en enkelt linje med overskriften.
            """;

        string articleContext =
            $"""
                Aritklen var udgivet på dette tidspunkt: {article.Published:o}.
                Artiklens sprog er: {article.ArticleLanguage}.
                Den originale kilde til artiklen er: {article.Source}.
                Den originale overskrift til artiklen er: {article.Headline}.
                Du må bruge den originale overskrift som inspiration til din overskrift, men husk at holde dig til de tidligere instruktioner.
            """;
        
        List<ChatMessage> messages =
        [
            new(ChatMessage.RoleEnum.System, dateContext),
            new(ChatMessage.RoleEnum.System, instructionsPrompt),
            new(ChatMessage.RoleEnum.System, articleContext),
            new(ChatMessage.RoleEnum.User, article.Text)
        ];
        
        ChatCompletionRequest request = new(ModelDefinitions.MistralLarge, messages)
        {
            Temperature = 0.5m
        };

        ChatCompletionResponse? response = await client.Completions.GetCompletionAsync(request);

        await Task.Delay(500);

        return $"\ud83c\uddea\ud83c\uddfa {response.Choices.FirstOrDefault()?.Message.Content.Split("\n").FirstOrDefault()}";
    }
}