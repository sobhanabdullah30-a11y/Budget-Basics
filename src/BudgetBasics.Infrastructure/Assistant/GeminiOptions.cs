namespace BudgetBasics.Infrastructure.Assistant;

public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    public bool Enabled { get; set; } = true;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-3.8-flash";
    public int TimeoutSeconds { get; set; } = 15;
    public int MaxOutputTokens { get; set; } = 350;
}
