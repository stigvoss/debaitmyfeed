using Microsoft.Extensions.Options;
using Mistral.SDK;
using Mistral.SDK.DTOs;

namespace DebaitMyFeed.Library.HeadlineStrategies.MistralAi;

public class MistralAiHeadlineStrategy : IHeadlineStrategy
{
    private readonly MistralClient client;
    private readonly string model;
    private readonly decimal temperature;

    public MistralAiHeadlineStrategy(IOptions<MistralAiOptions> options)
    {
        MistralAiOptions mistralAiOptions = options.Value;
        
        this.model = mistralAiOptions.Model;
        
        APIAuthentication authentication = new APIAuthentication(mistralAiOptions.ApiKey);
        this.client = new MistralClient(authentication);
        
        this.temperature = mistralAiOptions.Temperature;
    }

    public string Id => "mistralai";
    
    public byte MaxConcurrency => 3;

    public async Task<string?> GetHeadlineAsync(Article article, CancellationToken cancellationToken)
    {
        string dateContext =
            $"""
                The current date and time is: {DateTimeOffset.Now:o}.
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
                The article was published at: {article.Published:o}.
                The language of the article is: {article.ArticleLanguage}.
                The original source of the article is: {article.Source}.
                The original title of the article is: {article.Headline}.
            """;
        
        List<ChatMessage> messages =
        [
            new(ChatMessage.RoleEnum.System, dateContext),
            new(ChatMessage.RoleEnum.System, instructionsPrompt),
            new(ChatMessage.RoleEnum.System, articleContext),
            new(ChatMessage.RoleEnum.User, article.Text)
        ];
        
        ChatCompletionRequest request = new(this.model, messages)
        {
            Temperature = this.temperature
        };

        ChatCompletionResponse? response = await client.Completions.GetCompletionAsync(request, cancellationToken);

        await Task.Delay(500, cancellationToken);

        return $"\ud83c\uddea\ud83c\uddfa {response.Choices.FirstOrDefault()?.Message.Content.Split("\n").FirstOrDefault()}";
    }
}