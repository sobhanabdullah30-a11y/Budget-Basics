# Sitemap implementation checklist

This checklist maps the supplied BudgetBasics sitemap to the working MVC application.

| Sitemap requirement | MVC implementation | Backend / database |
|---|---|---|
| Landing page, main menu and calls to action | `Home/Index` and shared responsive layout | SQL-backed aggregate visitor counter |
| Budgeting basics | `#learn` concept cards and sample monthly budget | Server-rendered Razor content |
| Needs vs wants | `#needs-wants` interactive knowledge check | Browser interaction with accessible feedback |
| Savings goals | `#tools` savings goal form | MVC POST endpoint and domain calculation |
| Expense planner | `#tools` add/edit/remove table | Authenticated rows persist in SQL Server; anonymous rows remain temporary |
| Money mistakes | `#mistakes` five expandable scenarios | Server-rendered Razor content |
| Infographics and learning gallery | `#resources` four original CSS/HTML visuals | Server-rendered Razor content |
| Search, sort and filter | Resource search, topic filters and A-Z sorting | Progressive client-side interaction |
| AI Q&A assistant | `#assistant` BudgetBee chat UI | MVC POST endpoint with local, safe rule-based answers |
| About us | `Home/About` | MVC page |
| Feedback | `Home/Feedback` and landing-page form | Validated MVC endpoint and `FeedbackSubmissions` SQL table |
| Contact us | `Home/Contact` | MVC page |
| Educational disclaimer | `Home/Disclaimer` | MVC page |
| Privacy note | `Home/Privacy` | Documents stored and non-stored data |
| Sitemap | `Home/Sitemap` | Links to every required area |
| Back to top | Shared footer link | Works on every page |

## Database connectivity

- ASP.NET Core Identity tables store users and roles.
- `RefreshTokens` stores only SHA-256 token hashes for JWT refresh rotation.
- `SiteStatistics` stores the aggregate visit count.
- `ExpenseEntries` stores user-owned planner rows with a foreign key to `AspNetUsers`.
- `FeedbackSubmissions` stores validated feedback with an optional user foreign key.
- EF Core migration `AddExpensePlannerAndFeedback` creates the two feature tables and indexes.
