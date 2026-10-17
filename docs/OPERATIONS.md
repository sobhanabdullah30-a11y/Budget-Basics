# Production operations

This guide covers the IIS/MonsterASP production controls that complement the application code.

## CI/CD and version control

- `main` is the production branch.
- Every push and pull request runs restore, Release build, tests, coverage collection, and an IIS publish in `.github/workflows/ci.yml`.
- CodeQL scans C# changes and runs weekly. Dependabot checks NuGet and GitHub Actions dependencies weekly.
- Production deploys run only after CI succeeds and `ENABLE_MONSTER_DEPLOY` is `true`, or after an authorized manual dispatch.
- Deployments use the protected GitHub `production` environment and finish with an HTTPS `/health` check.

Configure GitHub before enabling automatic deployment:

1. Open **Settings → Environments → production** and add an optional required reviewer.
2. Add the environment secret `MONSTER_PUBLISH_PASSWORD` using the current MonsterASP Web Deploy password.
3. Add repository variable `ENABLE_MONSTER_DEPLOY` with value `true`.
4. Never upload the downloaded `.publishSettings` file or a `.pubxml.user` file.

If the password is rotated in MonsterASP, update the GitHub environment secret immediately.

## Security

- HTTPS redirection and a one-year HSTS policy are enabled outside Development.
- IIS and Kestrel server disclosure headers are removed where the host supports it.
- CSP, clickjacking, MIME sniffing, referrer, permissions, and cross-origin isolation headers are applied centrally.
- Account and authenticated responses are explicitly non-cacheable.
- ASP.NET Core Identity applies a strong password policy, email confirmation, and lockout protection.
- JWT access tokens are short-lived; refresh tokens are rotated and revocable.
- Antiforgery validation is global for unsafe MVC methods.

## Rate limiting

Limits are partitioned by authenticated subject or client IP:

| Policy | Default |
|---|---:|
| Global sliding window | 300 requests/minute |
| Login, registration, refresh | 10 requests/minute |
| BudgetBee assistant | 10 requests/minute |
| Planner and form APIs | 60 requests/minute |

Rejected requests return HTTP `429`, a JSON message, and `Retry-After` when available. Tune the values through the `RateLimiting__...` environment variables after reviewing production metrics.

## Caching and compression

- Brotli and Gzip compression are enabled for HTTPS responses.
- Public reference pages use server-side output caching.
- Student-tip reference data varies by the `habit` query value.
- Authenticated responses and account routes are never cached.
- Fingerprinted CSS and JavaScript assets use a one-year immutable browser/CDN cache policy.

Output cache is intentionally in-process because the current deployment is a single shared-IIS application. Use a distributed output-cache store before running multiple application instances.

## CDN activation

The application is CDN-ready but keeps same-origin assets as the safe default. A CDN cannot be provisioned from application code alone; it requires a CDN account or control of a custom domain.

After configuring a CDN pull zone whose origin is `https://budgetbasics-pk.runasp.net`:

1. Restrict the CDN to `/css`, `/js`, `/lib`, `/images`, and `/favicon.ico`.
2. Respect origin `Cache-Control` and query strings.
3. Do not cache `/Account`, `/tools`, `/health`, or HTML containing user state.
4. Set `Cdn__Enabled=true` in MonsterASP.
5. Set `Cdn__BaseUrl=https://your-cdn-host.example` in MonsterASP.
6. Restart the application and verify CSS/JS URLs, CSP, HTTPS, and cache headers.

Only HTTPS CDN origins without query strings or fragments pass startup validation.

## Monitoring and recovery

- `.github/workflows/production-monitor.yml` checks production every 30 minutes and can also be run manually. It verifies `/health`, the homepage, database-backed learning-tip data, HSTS, CSP, and MIME-sniffing protection. A failed run appears in GitHub Actions and uses the repository owner's normal Actions notification settings.
- The application must return `Healthy` from `/health` over HTTPS before and after each deployment.
- Preserve IIS/application logs without recording tokens, cookies, connection strings, or submitted financial values.
- MonsterASP keeps provider disaster-recovery copies, but application owners must also create their own database backups. Before a schema change, open **Databases → the BudgetBasics database → Backups → Create BAK file**, wait for completion, and download the `.bak` file to encrypted/off-site storage.
- Perform a quarterly restore drill against a separate non-production database. Upload the `.bak`, restore it with **Create a safety backup of the current data first** enabled, start a temporary application against that restored database, and verify the Identity, `Expenses`, `FeedbackSubmissions`, `RefreshTokens`, `LearningTips`, and `VisitorStatistics` tables. Never restore a drill over production.
- Record the backup filename, UTC creation time, file size, restore target, restore result, and tester in the release notes. A backup is not considered verified until this drill succeeds.
- The 2026-10-17 production backup passed `RESTORE VERIFYONLY` and a complete isolated local staging restore. Evidence is recorded in [`BACKUP-VERIFICATION-2026-10-17.md`](./BACKUP-VERIFICATION-2026-10-17.md).
- To roll back, redeploy a known-good Git tag or commit through the same production workflow.

### Production verification completed 2026-10-17

- Registration issued secure access and refresh cookies.
- Refresh-token rotation returned HTTP 204.
- The authenticated profile and expense API were accessible only while signed in.
- Expense create, read, update, persistence across logout/login, and delete passed against production SQL Server.
- Logout revoked the session; the protected endpoint returned HTTP 401 afterward.
- A marked QA feedback submission was saved successfully.
- BudgetBee returned an enhanced response, proving the configured server-side assistant integration was reachable without exposing its key.
- JWT signing and SQL connection secrets were functionally verified by the successful token and persistence flows.
