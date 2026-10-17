using BudgetBasics.Application.Assistant;

namespace BudgetBasics.IntegrationTests;

public sealed class BudgetBeeServiceTests
{
    private readonly BudgetBeeService _service = new();

    [Theory]
    [InlineData("What is the 50-30-20 rule?", "50-30-20 guideline")]
    [InlineData("How do I start an emergency fund?", "Emergency fund")]
    [InlineData("Mera kharcha kaise track hoga?", "Expense tracking")]
    [InlineData("How can I manage qarz?", "Debt basics")]
    [InlineData("How much should I save?", "Saving")]
    [InlineData("Is scholarship income?", "Income")]
    public void Answer_ReturnsRelevantEducationalTopic(string question, string expectedTopic)
    {
        var response = _service.Answer(question);

        Assert.Equal(expectedTopic, response.Topic);
        Assert.NotEmpty(response.Answer);
        Assert.Equal(3, response.Suggestions.Count);
    }

    [Fact]
    public void Answer_ForUnknownQuestion_ReturnsHelpAndSuggestions()
    {
        var response = _service.Answer("Tell me about astronomy");

        Assert.Equal("Help", response.Topic);
        Assert.Equal(3, response.Suggestions.Count);
    }
}
