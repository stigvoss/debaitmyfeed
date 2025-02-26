using System.ClientModel;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace DebaitMyFeed.Library.HeadlineStrategies.OpenAi;

public class OpenAiHeadlineStrategy : IHeadlineStrategy
{
    private readonly OpenAIClient openAiClient;
    private readonly ChatClient chatClient;
    private readonly float temperature;

    public OpenAiHeadlineStrategy(IOptions<OpenAiOptions> options)
    {
        OpenAiOptions openAiOptions = options.Value;
        
        this.openAiClient = new OpenAIClient(openAiOptions.ApiKey);
        this.chatClient = this.openAiClient.GetChatClient(openAiOptions.Model);

        this.temperature = openAiOptions.Temperature;
    }

    public string Id => "openai";

    public byte MaxConcurrency => 5;

    public async Task<string?> GetHeadlineAsync(FeedArticle article, CancellationToken cancellationToken)
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
                Du vil kun give overskriften, intet andet i dit svar.
            """;

        string articleContext =
            $"""
                Aritklen var udgivet på dette tidspunkt: {article.Published:o}.
                Artiklens sprog er: {article.ArticleLanguage}.
                Den originale kilde til artiklen er: {article.Source}.
                Den originale overskrift til artiklen er: {article.Headline}.
                Du må bruge den originale overskrift som inspiration til din overskrift, men husk at holde dig til de tidligere instruktioner.
            """;
        
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(dateContext),
            new SystemChatMessage(instructionsPrompt),
            new SystemChatMessage(articleContext),
            new UserChatMessage(article.Text)
        };

        ChatCompletionOptions options = new()
        {
            Temperature = this.temperature
        };
        ClientResult<ChatCompletion>? chatCompletion = await chatClient.CompleteChatAsync(messages, options, cancellationToken);

        return chatCompletion.Value.Content.FirstOrDefault()?.Text;
    }
}