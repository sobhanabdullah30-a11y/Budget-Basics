# Security policy

## Supported version

Security fixes are applied to the latest commit on the `main` branch and to the current production deployment.

## Reporting a vulnerability

Please do not disclose a suspected vulnerability in a public issue. Use GitHub's **Security → Report a vulnerability** flow for this repository. Include the affected route or component, reproduction steps, impact, and any safe proof of concept.

Never include real passwords, API keys, JWT signing keys, database connection strings, session cookies, or personal information in a report.

## Operational controls

- Secrets are supplied through local user secrets, MonsterASP environment variables, or protected GitHub environment secrets.
- Every state-changing MVC request is protected by antiforgery validation.
- Authentication endpoints, APIs, and the assistant have independent rate limits in addition to a global limit.
- Authenticated and account responses are marked `no-store`.
- Static assets use fingerprinted URLs and immutable cache headers.
- CodeQL and Dependabot monitor the public repository.
