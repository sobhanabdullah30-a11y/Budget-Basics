# Architecture and security design

## Onion Architecture

```text
BudgetBasics.Web (MVC presentation)
    |                    |
    v                    v
BudgetBasics.Application  BudgetBasics.Infrastructure
    |                    |
    +---------> BudgetBasics.Domain <---------+
```

The Domain project owns calculations and business invariants. Application defines use
cases, interfaces, policies, commands, and result models. Infrastructure implements SQL
Server, EF Core, ASP.NET Core Identity, JWT token handling, logging adapters, and seeded
content access. Web owns controllers, Razor Views, view models, filters, and static assets.

Infrastructure references Application and Domain; Application references Domain; Domain
has no project reference. MVC composes the system but business rules do not depend on MVC.

## Planned modules

1. Public shell: landing page, navigation, visitor count, clock, sitemap, about/contact.
2. Learn: Budgeting Basics, Needs vs Wants, Money Mistakes.
3. Practice: 50-30-20 calculator, Savings Goals, session-only Expense Planner.
4. Resources: infographics, searchable/filterable tips and examples.
5. Assistant: local rule-based educational chatbot with safe fallback.
6. Identity: registration, email confirmation, login, refresh, logout, account deletion.
7. Administration (provisional): curated content and chatbot response maintenance.
8. Operations: health checks, structured logs, metrics, backups, deployment migration.

## Initial relational model

```text
AspNetUsers 1 ----- * RefreshTokens
    |                    - TokenHash (unique; never store raw token)
    |                    - FamilyId (rotation/reuse detection)
    |                    - CreatedUtc / ExpiresUtc / RevokedUtc
    |
    * ----- * AspNetRoles (through AspNetUserRoles)

Future curated-content tables after content module approval:
LearningTopic 1 ----- * LearningContent
LearningTopic 1 ----- * ChatbotResponse
LearningTopic 1 ----- * Infographic
```

Personal income, goals, and expense-planner rows are deliberately absent. They remain in
the current browser/session to honor the SRS privacy and non-persistence constraints.

## JWT with an MVC frontend

The browser will not store tokens in `localStorage` or expose them to Razor JavaScript.
After a successful MVC login:

1. ASP.NET Core Identity verifies the normalized email, password hash, confirmed-email
   state, and lockout state.
2. The server issues a signed JWT access token lasting 15 minutes. Claims are limited to
   subject ID, display name, role, issued time, expiry, issuer, audience, and token ID.
3. The access JWT is sent in a `Secure`, `HttpOnly`, `SameSite=Lax` cookie named
   `bb_access`. The JwtBearer handler reads and validates that cookie on every protected
   request: signature, algorithm, issuer, audience, lifetime, and 30-second clock skew.
4. A cryptographically random opaque refresh token is placed in a separate `Secure`,
   `HttpOnly` cookie scoped to the refresh endpoint. Only its SHA-256 hash is stored in
   SQL Server.
5. Refresh rotates both tokens. The old record is revoked and points to the replacement.
   Reuse of an already-rotated token revokes its entire family and requires login again.
6. Logout revokes the current refresh family and expires both cookies. Password change,
   account disablement, or suspected compromise can revoke every active refresh token.
7. Because cookies are sent automatically, every unsafe MVC request also requires an
   antiforgery token. SameSite is defense-in-depth, not a replacement for antiforgery.
8. HTTPS/HSTS is mandatory outside development. Secrets come from user-secrets locally
   and App Service settings or Key Vault in Azure, never from committed JSON.

Authorization will use named policies from Application. Anonymous access remains the
default for all SRS-required educational modules. `AdministratorOnly` is provisional.

## Validation and errors

- View models validate shape and required input; Domain objects enforce business rules.
- Controllers redisplay accessible validation summaries and never trust browser-only
  validation.
- Expected failures return friendly messages. Unexpected failures use the central error
  handler, a correlation ID, and structured logs without personal or token data.
- Calculations use `decimal`, explicit rounding, invariant tests, and educational labels.

## Logging and configuration

- `ILogger<T>` structured events; never log passwords, JWTs, refresh tokens, connection
  strings, contact text, or full email addresses.
- Development configuration: `appsettings.json` plus .NET user-secrets.
- Production: environment/App Service settings; prefer Azure managed identity for SQL.
- Configuration is validated at startup. The application refuses a missing/short JWT key.

## Deployment recommendation and cost gate

Recommended learning deployment: Azure App Service plus Azure SQL Database because both
match the requested Microsoft stack and Visual Studio publishing workflow.

- App Service Free F1 is $0, shared, limited to 60 CPU minutes/day and 1 GB storage. It
  has no financially backed SLA and is appropriate only for an early demonstration.
- Azure SQL's current free offer includes 100,000 vCore-seconds, 32 GB data, and 32 GB
  backup storage per database each month, subject to subscription/offer eligibility.
- Production custom-domain, SLA, scale, staging-slot, or always-on requirements generally
  require a paid App Service tier. Exact pricing varies by region/currency and must be
  confirmed in Azure Pricing Calculator before creation.

No paid resource will be created without the owner selecting an Azure subscription,
region, resource names, and approving the portal's estimated monthly total. The planned
production connection uses App Service managed identity rather than a SQL password.

## Repeatable release process

1. Restore, build with warnings as errors, run unit/integration tests.
2. Run browser checks and Lighthouse; record results.
3. Produce a release build and an EF migration bundle.
4. Back up/export the database or confirm point-in-time restore retention.
5. Apply the migration in a controlled deployment step.
6. Deploy the immutable application artifact.
7. Verify `/health`, smoke tests, logs, HTTPS redirect, and security headers.
8. Roll back the application artifact on failure; use a forward database fix unless a
   tested rollback migration is safe.

