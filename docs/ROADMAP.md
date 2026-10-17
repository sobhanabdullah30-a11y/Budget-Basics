# Development roadmap

Every milestone ends with build/test evidence, an SRS checklist update, and—after the
deployment account is connected—a deployment to the same live test environment.

## Milestone 0 - Foundation (in progress)

- [x] Review and visually inspect all 15 SRS pages
- [x] Record conflicts and open decisions
- [x] Create .NET 10 Onion Architecture solution
- [x] Add MVC, SQL Server, EF Core, Identity, JWT validation packages
- [x] Add secure configuration baseline and health endpoint
- [x] Add first domain calculations and tests
- [x] Build with zero warnings and run all six tests successfully
- [x] Create initial EF Identity/refresh-token migration
- [ ] Create live Azure resources after cost/account approval

Verification: `dotnet build BudgetBasics.slnx` and `dotnet test BudgetBasics.slnx`.

## Milestone 1 - Identity and secure session (complete locally)

- Registration, email confirmation, login, access-token issue, refresh rotation, logout
- Account lockout, password reset, account deletion, token-family revocation
- MVC Razor forms with antiforgery and accessible validation
- Administrator seed procedure with no committed password
- Authentication integration and security-boundary tests

## Milestone 2 - Public learning foundation (complete locally)

- Branded responsive layout, homepage, navigation, sitemap, footer, clock, counter
- Budgeting Basics, sample monthly budget, knowledge check
- Needs vs Wants classification and decision guide
- Responsive/keyboard/contrast testing

## Milestone 3 - Planning tools (complete locally)

- 50-30-20 calculator and disclaimer
- Savings goal estimator and progress indicator
- Session-only expense planner with all seven categories, edit/remove, totals
- Formula, validation, localization, and boundary tests

## Milestone 4 - Content and discovery (complete locally)

- Money Mistakes scenarios/accordions
- Original infographic gallery with captions and alt text
- Search, sort, filters, and no-results behavior
- Optional admin content editing only after role confirmation

## Milestone 5 - Assistant and forms (complete locally)

- Local JSON/rule-based educational chatbot, prompt suggestions, fallback, disclaimer
- About, contact details, feedback validation/confirmation with no transmission
- Privacy notice and AI-tools acknowledgement

## Milestone 6 - Quality and release

- Error handling, structured logging, security headers, rate limits
- Cross-browser, responsive, accessibility, performance, SEO, and Lighthouse checks
- Azure monitoring, alerting, backup/restore drill, migration bundle, CI/CD
- Code-free report, diagrams, test data, installation guide, `ReadMe.doc`, ZIP
- Mandatory MP4 demonstration and final live URL

## Information needed from the owner

1. Confirm whether Administrator and optional Learner are the only authenticated roles.
2. Provide creator/about text, public contact details, and approved social links.
3. Choose Azure subscription and region; approve the displayed cost before provisioning.
4. Provide a sending domain/email service only if email confirmation is required online.
5. Confirm whether feedback must remain local-only (the SRS default) or be stored/sent.
