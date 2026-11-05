# منبع — Manbaa API

Backend for منبع, an educational platform for Egyptian teachers and centers: secretary work, teaching, and follow-up for students and parents.

.NET 10 · ASP.NET Core · EF Core · SQL Server · SignalR · JWT

## Run it

1. Copy `src/Manbaa.Api/appsettings.Example.json` to `src/Manbaa.Api/appsettings.Development.json` and fill in the connection string, a long JWT key and the first system admin's email and password.
2. Create the database:
   ```bash
   dotnet tool install --global dotnet-ef
   dotnet ef database update --project src/Manbaa.Infrastructure --startup-project src/Manbaa.Infrastructure --connection "<your connection string>"
   ```
3. `dotnet run --project src/Manbaa.Api` and open `/swagger`. On first start the roles and the system admin are created.

## Tests

```bash
dotnet test
```

## Docs

- [docs/Architecture.md](docs/Architecture.md) — structure, tenancy, audit log, notifications, subscriptions
- [docs/API.md](docs/API.md) — endpoints
