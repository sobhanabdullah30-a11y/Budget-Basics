# SRS traceability — current implementation

Source reviewed in full: `BudgetBasics-Web Innovation Unleashed_SRS.pdf`, version 1.0, 15 pages.
The user's later ASP.NET Core MVC, SQL Server, JWT, and live-hosting requirements formally extend the SRS's original frontend-only scope.

Status keys: **Complete**, **Partial**, **Excluded**.

| Area | SRS requirement | Implementation and verification | Status |
|---|---|---|---|
| Global | Brand, hero, welcome content, CTA, responsive navigation | Shared Razor layout and responsive home hero | Complete |
| Global | Featured tips/facts, visitor count, live date/time | Tip ticker, SQL aggregate counter, locale-aware live clock | Complete |
| Global | Sitemap and footer links | `/Home/Sitemap` plus footer routes | Complete |
| Learning | Income, fixed/variable expenses, needs/wants, savings | Searchable learning cards and labeled examples | Complete |
| Learning | Student monthly budget example | PKR 30,000 sample and 50/30/20 bars | Complete |
| Learning | Needs/wants classification and answer feedback | Six-question interactive activity with explanations and score | Complete |
| Learning | Delay-purchase decision guide | Three-question pause guide | Complete |
| Planning | 50/30/20 explanation and calculator | Domain calculation, validated POST, labeled result and disclaimer | Complete |
| Planning | Savings goal estimate | Validated inputs, remaining amount, months, progress and tip | Complete |
| Planning | Expense planner | Seven categories, add/edit/remove, totals; session mode for guests and SQL persistence for accounts | Complete |
| Learning | Five common money mistakes | Five accessible expandable scenarios with corrections | Complete |
| Resources | Four original infographic topics | Responsive HTML/CSS graphics with figure captions | Complete |
| Resources | Search, sorting, topic filters, no-results state | Client-side accessible controls and status | Complete |
| Assistant | Budgeting Q&A, prompts, fallback and disclaimer | BudgetBee endpoint, local rule-based fallback, optional AI enhancement, rate limiting | Complete |
| Guidance | Preloaded tips for common spending habits | `LearningTips` SQL table seeded through EF migration; anonymous read endpoint | Complete |
| Connect | About, feedback, contact and confirmation | About/contact pages; feedback stored in SQL; validated contact demo; safe social-share links | Complete |
| Contact | Public project email and phone | Contact form is the public channel; phone is listed as available on request because the owner has not approved personal contact details for publication | Partial |
| Experience | Hover/focus states, dark mode, animation, ticker, back-to-top | Theme-aware, responsive and reduced-motion-aware implementation | Complete |
| Accessibility | Semantic controls, keyboard use, labels and status messages | Razor/HTML implementation plus manual/browser smoke coverage | Complete |
| Quality | Current browsers, responsive layouts and Lighthouse evidence | Live Lighthouse: Performance 99, Accessibility 100, Best Practices 100, SEO 100; Chrome/Edge responsive smoke checks complete | Complete |
| Security | JWT validation, expiry, refresh rotation, logout, role policy | HttpOnly/Secure cookies, token family revocation, Identity lockout and email-confirmation flow | Complete |
| Architecture | Onion layers, SQL Server and EF Core migrations | Domain → Application ← Infrastructure; MVC composition root | Complete |
| Hosting | HTTPS live deployment and repeatable publish flow | MonsterASP.NET IIS deployment at `https://budgetbasics-pk.runasp.net` | Complete |
| Deliverables | Report, video, ZIP, ReadMe.doc and other submission artifacts | Explicitly excluded by the owner from this completion pass | Excluded |

## Current automated checks

- Domain calculation tests cover normal and boundary cases.
- MVC integration tests cover health, homepage learning features, and invalid tip input.
- JavaScript syntax is checked with Node.
- Production smoke tests cover public pages, calculator endpoints, assistant response, and seeded-tip retrieval.
- The final live Lighthouse audit records 1.7 s LCP, 40 ms total blocking time, and zero cumulative layout shift.

The only content-level limitation is public email/phone disclosure. No personal address or phone number is published without explicit owner approval.
