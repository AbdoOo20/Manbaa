# Manbaa backend

Educational platform for the Egyptian market (teachers, secretaries, students, parents). .NET 10, SQL Server, same structure as Tamam (https://github.com/AbdoOo20/Tamam).

Read `docs/Architecture.md` before adding code, and update `docs/API.md` with every endpoint you add or change.

## Rules

- Layers: `Api -> Infrastructure -> Application -> Domain`. Business logic over `IUnitOfWork` goes in Application; anything needing Identity or another library goes in Infrastructure.
- Controllers are thin: call one service and `Respond(result)`. Admin endpoints inherit `AdminControllerBase` (`/api/admin`, `SystemAdmin` only).
- Services return `Result` / `Result<T>`. User-facing messages are in Egyptian Arabic; logs in English.
- Every entity that belongs to a teacher implements `ITenantEntity`. Never query across teachers in a request a teacher can call.
- Every change is audited automatically. Never mark a business entity `[NotAudited]`, and never edit or delete `AuditLog` rows.
- Never hard-delete business data (soft delete) and never edit money records: cancel them with a reason.
- Use `TimeProvider`, never `DateTime.UtcNow`.
- Notifications: `INotificationService.QueueAsync(...)` before `SaveChangesAsync`; they are pushed over SignalR automatically.
- One type per file, file-scoped namespaces, blank line between statements, no unused usings. The build must have zero warnings.
- Add tests in `tests/Manbaa.Tests` for business rules (they run the real services on in-memory SQLite).

## Commands

```bash
dotnet build
dotnet test
dotnet ef migrations add <Name> --project src/Manbaa.Infrastructure --startup-project src/Manbaa.Infrastructure --output-dir Persistence/Migrations
dotnet ef database update --project src/Manbaa.Infrastructure --startup-project src/Manbaa.Infrastructure --connection "<connection string>"
```

`src/Manbaa.Api/appsettings.json` and `appsettings.Development.json` are not committed; copy `appsettings.Example.json`.
