using Mistral.SDK;

namespace DebaitMyFeed.Library.HeadlineStrategies.MistralAi;

public class MistralAiOptions
{
    public required string ApiKey { get; set; }
    public string Model { get; set; } = ModelDefinitions.MistralLarge;
    public decimal Temperature { get; set; } = 0.5m;
}