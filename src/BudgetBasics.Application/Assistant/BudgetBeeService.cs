namespace BudgetBasics.Application.Assistant;

/// <summary>
/// Provides deterministic educational guidance without sending visitor
/// questions to an external AI provider.
/// </summary>
public sealed class BudgetBeeService
{
    private static readonly string[] DefaultSuggestions =
    [
        "What is the 50-30-20 rule?",
        "How can I stop overspending?",
        "How do I start an emergency fund?"
    ];

    public BudgetBeeResponse Answer(string question)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        var text = question.Trim().ToLowerInvariant();

        if (IsGreeting(text))
            return Response("Hello! I can explain budgeting, savings, expenses, needs and wants, debt, emergency funds, and the 50-30-20 guideline.", "Welcome", "What is a budget?", "How much should I save?", "What is a need?");

        if ((ContainsAny(text, "need", "zaroorat") && ContainsAny(text, "want", "shoq")) || ContainsAny(text, "what is a need", "need kya"))
            return Response("A need is essential for daily life, such as basic food, medicine, housing, safe transport, or required study material. A want adds comfort or enjoyment but can usually wait. When unsure, ask: do I need it now, can I afford it, and will I still want it tomorrow?", "Needs and wants", "Give me examples of needs", "How do I control wants?", "What is the 50-30-20 rule?");

        if (ContainsAny(text, "50-30-20", "50 30 20", "50/30/20"))
            return Response("The 50-30-20 guideline suggests using about 50% of income for needs, 30% for wants, and 20% for savings or debt repayment. It is a flexible educational starting point—adjust it when essential costs are higher.", "50-30-20 guideline", "How much should I save?", "What counts as a need?", "How do I make a monthly budget?");

        if (ContainsAny(text, "emergency", "unexpected", "medical emergency"))
            return Response("Start an emergency fund with a small, reachable target such as one week of essential expenses. Keep it separate from spending money, contribute regularly, and gradually work toward several months of essential costs.", "Emergency fund", "How much should I save monthly?", "How do I set a savings goal?", "How can I reduce expenses?");

        if (ContainsAny(text, "debt", "loan", "qarz", "credit card"))
            return Response("List every debt with its balance, minimum payment, due date, and cost. Pay minimums on time, avoid new unnecessary borrowing, then direct extra money to one balance at a time. Contact the lender early if you may miss a payment.", "Debt basics", "How do I make a budget?", "How can I reduce expenses?", "What is an emergency fund?");

        if (ContainsAny(text, "overspend", "over spend", "impulse", "control spending", "fazool", "extra kharch"))
            return Response("Set a weekly limit for flexible spending, track small purchases, and use a 24-hour pause before non-essential purchases. Move savings aside when income arrives instead of waiting to see what remains.", "Spending habits", "How do I track expenses?", "What is a want?", "How can I save first?");

        if (ContainsAny(text, "expense", "expenses", "track", "kharcha", "kharach"))
            return Response("Track the date, category, description, and amount of each expense. Review totals weekly, compare them with your plan, and adjust the categories that repeatedly go over their limits. You can use the Expense Planner on this page.", "Expense tracking", "How do I reduce expenses?", "How do I make a budget?", "How can I avoid overspending?");

        if (ContainsAny(text, "saving goal", "savings goal", "save for", "goal"))
            return Response("Make the goal specific: name it, choose a target amount and date, subtract what you already have, then divide the remainder into realistic monthly contributions. Use the Savings Goal calculator above to estimate the time required.", "Savings goals", "How much should I save?", "What is an emergency fund?", "How can I spend less?");

        if (ContainsAny(text, "how much", "save", "saving", "savings", "bachat"))
            return Response("A common learning guideline is 20% of income, but the best amount is one you can contribute consistently after essential costs. Start small if needed, separate it from spending money, and increase it when your income grows.", "Saving", "How do I set a savings goal?", "What is an emergency fund?", "What is the 50-30-20 rule?");

        if (ContainsAny(text, "income", "salary", "allowance", "amdani"))
            return Response("Income is money you receive, such as an allowance, scholarship, salary, freelance payment, or family support. For planning, use reliable after-deduction income and avoid treating borrowed money as income.", "Income", "How do I make a budget?", "What are fixed expenses?", "How much should I save?");

        if (ContainsAny(text, "budget", "plan", "monthly plan"))
            return Response("A budget is a plan for your expected income, spending, and saving. List income first, then essential fixed costs, flexible costs, debt payments, and one savings goal. Review the plan regularly and change it when real life changes.", "Budgeting basics", "What is the 50-30-20 rule?", "How do I track expenses?", "How much should I save?");

        return new BudgetBeeResponse(
            "I could not match that question to a BudgetBasics topic. I can help with budgeting, income, expenses, needs and wants, savings goals, emergency funds, debt basics, and overspending. Please choose a suggestion or ask using a few simple words.",
            "Help",
            DefaultSuggestions);
    }

    private static bool ContainsAny(string text, params string[] terms) =>
        terms.Any(term => text.Contains(term, StringComparison.OrdinalIgnoreCase));

    private static bool IsGreeting(string text) =>
        text is "hi" or "hello" or "salam" or "assalam" or "assalam o alaikum" ||
        text.StartsWith("hi ", StringComparison.Ordinal) ||
        text.StartsWith("hello ", StringComparison.Ordinal) ||
        text.StartsWith("salam ", StringComparison.Ordinal) ||
        text.StartsWith("assalam ", StringComparison.Ordinal);

    private static BudgetBeeResponse Response(string answer, string topic, params string[] suggestions) =>
        new(answer, topic, suggestions);
}
