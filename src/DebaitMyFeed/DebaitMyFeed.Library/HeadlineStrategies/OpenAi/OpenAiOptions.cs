namespace DebaitMyFeed.Library.HeadlineStrategies.OpenAi;

public class OpenAiOptions
{
    public required string ApiKey { get; set; }
    public string Model { get; set; } = "gpt-4o-mini";
    public float Temperature { get; set; } = 0.5f;
}