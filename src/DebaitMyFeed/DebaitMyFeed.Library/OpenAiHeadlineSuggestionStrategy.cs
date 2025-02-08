using System.ClientModel;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace DebaitMyFeed.Library;

public class OpenAiOptions
{
    public required string ApiKey { get; set; }
    public string Model { get; set; } = "gpt-4o-mini";
}

public class OpenAiHeadlineSuggestionStrategy : IHeadlineSuggestionStrategy
{
    private readonly OpenAIClient openAiClient;
    private readonly ChatClient chatClient;

    public OpenAiHeadlineSuggestionStrategy(IOptions<OpenAiOptions> options)
    {
        OpenAiOptions openAiOptions = options.Value;
        
        this.openAiClient = new OpenAIClient(openAiOptions.ApiKey);
        this.chatClient = this.openAiClient.GetChatClient(openAiOptions.Model);
    }
    
    public async Task<string?> SuggestHeadlineAsync(string articleText)
    {
        string prompt = 
            """
                The user will provide you the text of the article.
                You will use the text to generate a clear, concise, specific 
                and descriptive headline free of clickbait for the article.
                Focus on key points of the article.
                Keep a neutral tone. 
                If the key people in the article are expected to be known by the reader, consider including their name,
                otherwise, if key people has a title significant to the article, consider including the title.
                If the article mentions a location key to the article, consider including the location.
                If the article mentions commonly known organizations, consider including the organization.
                Keep the headline in the language of the article. 
                You will only give the headline, nothing else.
            """;
        
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(prompt),
            new UserChatMessage(articleText)
        };

        ChatCompletionOptions options = new()
        {
            Temperature = 0.5f
        };
        ClientResult<ChatCompletion>? chatCompletion = await chatClient.CompleteChatAsync(messages, options);

        return chatCompletion.Value.Content.FirstOrDefault()?.Text;
    }

    public async Task<string?> SuggestHeadlineAsync(Article article)
    {
        string dateContext =
            $"""
                Current date and time is {DateTimeOffset.Now:o}.
            """;
        
        string instructions = 
            """
                The user will provide you the text of the article.
                You will use the text to generate a clear, concise, specific 
                and descriptive headline free of clickbait for the article.
                Focus on key points of the article.
                Keep a neutral tone. 
                If the key people in the article are expected to be known by the reader, consider including their name,
                otherwise, if key people has a title significant to the article, consider including the title.
                If the article mentions a location key to the article, consider including the location.
                If the article mentions commonly known organizations, consider including the organization.
                Keep the headline in the language of the article. 
                You will only give the headline, nothing else.
            """;

        string articleContext =
            $"""
                The article was published on {article.Published:o}.
                The article is in {article.ArticleLanguage}.
            """;
        
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(dateContext),
            new SystemChatMessage(instructions),
            new SystemChatMessage(articleContext),
            new UserChatMessage(article.Text)
        };

        ChatCompletionOptions options = new()
        {
            Temperature = 0.5f
        };
        ClientResult<ChatCompletion>? chatCompletion = await chatClient.CompleteChatAsync(messages, options);

        return chatCompletion.Value.Content.FirstOrDefault()?.Text;
    }
}