namespace Infrustructure;

public sealed class AIOptions
{
    public string ChatModel { get; set; } = string.Empty;

    public string EmbeddingModel { get; set; } = string.Empty;

    public int EmbeddingDimentions { get; set; }

    public string ParserFilePath { get; set; } = string.Empty;

    public int MaxTokens { get; set; }

    public int OverlapTokens { get; set; }
}