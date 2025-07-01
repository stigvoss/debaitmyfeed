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

    public async Task<string?> GetHeadlineAsync(Article article, CancellationToken cancellationToken)
    {
        string dateContext =
            $"""
                Nuværende dato og klokkeslæt er {DateTimeOffset.Now:o}.
            """;
        
        string instructionsPrompt = 
            """
                <role>
                The user will provide the article text.
                You will use the text to generate a clear, precise, specific, and descriptive headline for the article that is free from clickbait.
                </role>
                <goal>
                Concisely inform the reader about the content of the news. The reader should be able to understand the context of the news only by the title.
                </goal>
                <rules>
                1. Focus on the key points in the article.
                2. The tone of the headline should be neutral.
                3. If key individuals in the article are expected to be known by the reader, consider including their name, or if key individuals have a title that is significant to the article, consider including the title.
                4. If it is more likely that the reader will recognize the person’s title rather than their name, then consider using the title instead of the name.
                5. If the person behind the title is very well known, e.g. President of the USA, then use the title instead of the name.
                6. If the article content concerns specific key locations, consider including the location in the headline.
                7. If the article mentions widely known organizations or companies, consider including the name in the headline, as long as it is relevant to the core content of the article.
                8. Use *ONLY* the article’s original language when creating the headline.
                9. You will *ONLY* provide the headline, nothing else in your response.
                10. If the article language is not English, *DO NOT* use the English word "amid".
                </rules>
                Wait. Are you following all the rules for your role?
            """;

        string articleContext =
            $"""
                Aritklen var udgivet på dette tidspunkt: {article.Published:o}.
                Artiklens sprog er: {article.ArticleLanguage}.
                Den originale kilde til artiklen er: {article.Source}.
                Den originale overskrift til artiklen er: {article.Headline}.
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