# Production database backup verification — 2026-10-17

## Result

**Passed.** The MonsterASP SQL Server backup was verified without touching the live database.

| Check | Result |
|---|---|
| Backup file | `db71484_custom_17.10.2026_639278635994747545.bak` |
| File size | 6,279,168 bytes (5.99 MiB) |
| SHA-256 | `B3F60CFD4995EEA37B5488D2E3B406F27F3DF33BD147F71AB199A420FFEAF710` |
| SQL backup database | `db71484` |
| Backup type | Full database |
| Backup start | 2026-10-17 19:53:19 (server-reported time; timezone not embedded) |
| Backup finish | 2026-10-17 19:53:19 (server-reported time; timezone not embedded) |
| Header damage flag | False |
| `RESTORE VERIFYONLY` | Passed |
| Isolated staging restore | Passed |
| Temporary database cleanup | Passed |
| Temporary backup-copy cleanup | Passed |

The hosting-generated backup does not contain SQL backup checksums, so SQL Server correctly rejected `RESTORE VERIFYONLY ... WITH CHECKSUM`. The compatible `RESTORE VERIFYONLY` command completed successfully, followed by a stronger full restore drill.

## Restored schema evidence

The backup was restored into a uniquely named local SQL Server Express staging database. Required tables were present and readable:

| Table | Rows at backup time |
|---|---:|
| `AspNetUsers` | 6 |
| `RefreshTokens` | 11 |
| `ExpenseEntries` | 1 |
| `FeedbackSubmissions` | 2 |
| `LearningTips` | 12 |
| `SiteStatistics` | 1 |
| `__EFMigrationsHistory` | 4 |

Row counts are recorded only as recovery evidence; no personal records, credentials, tokens, or submitted text were exported into this report.

## Safety controls

- The production MonsterASP database was never connected to or restored over.
- The original `.bak` file remained unchanged.
- The restore target used a generated `BudgetBasicsRestoreVerify_...` database name.
- The temporary staging database was dropped after verification.
- Temporary copies used to give the SQL Server service file access were removed.
- The backup itself remains outside source control and must never be committed to Git.

## Next drill

Repeat this verification after a major schema migration and at least quarterly. Create a fresh backup first and compare its SHA-256 value with the downloaded file before beginning the restore drill.
