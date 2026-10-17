using System.Net.Http.Json;
using System.Text.Json;
using BudgetBasics.Application.Assistant;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BudgetBasics.Infrastructure.Assistant;

public sealed class GeminiBudgetBeeService(
    HttpClient httpClient,
    IOptions<GeminiOptions> options,
    BudgetBeeService localAssistant,
    ILogger<GeminiBudgetBeeService> logger) : IBudgetBeeService
{
    private const string SystemInstruction = """
        You are BudgetBee, the educational assistant inside BudgetBasics, a student budgeting website.
        Answer only questions about budgeting, saving, income, expenses, needs versus wants, debt basics,
        emergency funds, and healthy spending habits. Be friendly, practical, and beginner-friendly.
        Reply in the user's language; understand English and Roman Urdu. Keep the answer under 160 words.
        Never ask for or repeat bank details, card numbers, passwords, API keys, national identity numbers,
        exact account balances, or other sensitive personal information. Do not perform transactions.
        Do not claim to be a financial adviser. For high-stakes investment, tax, legal, or debt-crisis
        questions, give only general education and recommend a qualified local professional.
        If the question is outside personal-budget education, politely state the supported topics.
        Do not use Markdown tables, headings, or links. Return plain text only.
        """;

    public async Task<BudgetBeeResponse> AnswerAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        var fallback = localAssistant.Answer(question);
        var settings = options.Value;
        if (!settings.Enabled || string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            return fallback;
        }

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(settings.TimeoutSeconds, 5, 30)));

            var requestBody = new
            {
                systemInstruction = new
                {
                    parts = new[] { new { text = SystemInstruction } }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = question.Trim() } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.25,
                    maxOutputTokens = Math.Clamp(settings.MaxOutputTokens, 100, 500)
                }
            };

            var model = Uri.EscapeDataString(settings.Model.Trim());
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"v1beta/models/{model}:generateContent");
            request.Headers.Add("x-goog-api-key", settings.ApiKey);
            request.Content = JsonContent.Create(requestBody);

            using var response = await httpClient.SendAsync(request, timeout.Token);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Gemini returned HTTP {StatusCode}; BudgetBee used its local answer.",
                    (int)response.StatusCode);
                return fallback;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: timeout.Token);
            var answer = ReadAnswer(document.RootElement);
            if (string.IsNullOrWhiteSpace(answer))
            {
                logger.LogWarning("Gemini returned no usable text; BudgetBee used its local answer.");
                return fallback;
            }

            return new BudgetBeeResponse(
                answer.Trim(),
                "BudgetBee guidance",
                fallback.Suggestions);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("Gemini timed out; BudgetBee used its local answer.");
            return fallback;
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Gemini was unavailable; BudgetBee used its local answer.");
            return fallback;
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Gemini returned invalid JSON; BudgetBee used its local answer.");
            return fallback;
        }
    }

    private static string? ReadAnswer(JsonElement root)
    {
        if (!root.TryGetProperty("candidates", out var candidates) ||
            candidates.ValueKind != JsonValueKind.Array ||
            candidates.GetArrayLength() == 0)
        {
            return null;
        }

        var candidate = candidates[0];
        if (!candidate.TryGetProperty("content", out var content) ||
            !content.TryGetProperty("parts", out var parts) ||
            parts.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        var textParts = parts.EnumerateArray()
            .Where(part => part.TryGetProperty("text", out _))
            .Select(part => part.GetProperty("text").GetString())
            .Where(text => !string.IsNullOrWhiteSpace(text));
        return string.Join(Environment.NewLine, textParts);
    }
}
