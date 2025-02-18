namespace DebaitMyFeed.Library.HeadlineStrategies.Ollama;

public class OllamaOptions
{
    public required Uri Endpoint { get; set; }
    
    public required string Model { get; set; } = "llama3.2";
}