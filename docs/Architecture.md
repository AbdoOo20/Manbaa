# Manbaa – Project Structure

Same structure as Tamam. Read this before adding code.

## Layers (dependencies point inwards)

`Api -> Infrastructure -> Application -> Domain`

| Layer | Responsibility |
|---|---|
| **Domain** | Entities, enums, constants, domain exceptions. No dependency on other layers. |
| **Application** | Contracts (`Abstractions`), DTOs, service interfaces and business-logic services, mappings, settings. |
| **Infrastructure** | EF Core (`Persistence`), interceptors, implementations that need external libraries (Identity/JWT, storage, messaging). |
| **Api** | Controllers, SignalR hubs, background workers, middleware, OpenAPI, services that need `HttpContext`. |

## Folder map

```
src/
  Manbaa.Domain
    Common/            BaseEntity<TKey>, IAuditable, ISoftDeletable, ISearchable, ITenantEntity, NotAuditedAttribute, ArabicSearch
    Constants/         RoleNames, AuditActions
    Entities/<Area>/   one class per file: Identity, Teachers, Subscriptions, System
    Enums/             one enum per file
    Exceptions/

  Manbaa.Application
    Abstractions/      IRepository, IUnitOfWork, IUserAppService, ITeacherAccessService, INotificationPublisher, AppClaimTypes
    Common/            Result<T>, PagedQuery, PagedResult, QueryableExtensions, PhoneHelper
    DTOs/<Feature>/    request/response records, one type per file
    Services/<Feature>/ I<Feature>Service (+ implementation when it needs no external library)
    Mappings/          entity -> DTO projections
    Settings/          JwtSettings, SystemAdminSettings, SubscriptionSettings
    Exceptions/        AppException and friends (mapped to the unified response)

  Manbaa.Infrastructure
    Persistence/ManbaaDbContext.cs
    Persistence/Configurations/  IEntityTypeConfiguration<T>, one per entity
    Persistence/Interceptors/    AuditableEntity, AuditTrail, NotificationDispatch
    Persistence/Repositories/    GenericRepository, UnitOfWork
    Persistence/Migrations/      generated
    Services/<Feature>/          implementations that need Identity or other libraries
    Seeding/                     roles + first system admin

  Manbaa.Api
    Controllers/         thin controllers; Controllers/Admin/ for /api/admin/*
    Hubs/                NotificationsHub (SignalR) + SignalRNotificationPublisher
    BackgroundServices/  SubscriptionReminderWorker
    Extensions/ ExceptionHandling/ OpenApi/ RateLimiting/ Services/

tests/
  Manbaa.Tests           real services over in-memory SQLite
```

## Where does a new service go?

1. Interface + DTOs: `Application/Services/<Feature>` and `Application/DTOs/<Feature>`.
2. Implementation:
   - pure business logic over `IUnitOfWork` -> `Application/Services/<Feature>`
   - needs an external library (Identity, storage, WhatsApp) -> `Infrastructure/Services/<Feature>`
   - needs `HttpContext` -> `Api/Services`
3. Register it in `DependencyInjection.cs` of the layer that owns the implementation.

## Request flow and responses

`Controller -> Service (Application) -> IUnitOfWork -> DbContext (+ interceptors)`

- Controllers inherit `ApiControllerBase` (or `AdminControllerBase` for `/api/admin`), never touch `IUnitOfWork`, and only call a service then `Respond(result)`.
- Every service returns `Result` / `Result<T>`. Every HTTP response has the same JSON shape: `{ succeeded, statusCode, message, errors, data }`.
- User-facing messages (results, validation) are in Egyptian Arabic. Log messages stay in English.
- Enums are sent as strings (`"Trial"`, `"ExpiringSoon"`).

## Multi-tenancy (one teacher = one tenant)

- Every row that belongs to a teacher implements `ITenantEntity` (`TeacherId`).
- `ManbaaDbContext` adds a named query filter `Tenant` to those entities: a signed-in teacher, secretary, student or parent only ever reads their own teacher's rows. The system admin and background jobs see everything.
- `AuditableEntityInterceptor` fills `TeacherId` on new rows and refuses (403) saving a row of another teacher.
- Every user except the system admin has `User.TeacherId`. The JWT carries it as `teacher_id`.
- When a teacher's subscription ends (after the grace days) or the teacher is suspended, every token of that teacher is rejected (`ITeacherAccessService`, checked on each request and cached 30 seconds).
- To bypass a filter on purpose: `IgnoreQueryFilters([ManbaaDbContext.TenantFilter])`. Never do this in a request a teacher can call.

## Audit log

- `AuditTrailInterceptor` writes one `AuditLog` row for every create, update and delete of every entity, in the same transaction as the change: user, role, teacher, IP, user agent, time, and the values before and after.
- Opt out only for log-like entities with `[NotAudited]` (`AuditLog`, `Notification`).
- Logins and failed logins are logged by `AuthService`.
- Audit rows are append-only: the interceptor refuses to change or delete them, and the `TR_AuditLogs_ReadOnly` trigger blocks it in SQL Server too.
- Deletes are soft (`ISoftDeletable`): rows stay in the table and disappear from queries.
- Money records are never edited: a wrong payment is cancelled with a reason (`TeacherSubscription.IsCancelled`).

## Notifications (SignalR)

- Services call `INotificationService.QueueAsync(userIds, ...)`; the rows are saved with the service's next `SaveChangesAsync`.
- `NotificationDispatchInterceptor` pushes each saved notification to `NotificationsHub` group `user:{id}` with the method `notification`.
- The browser connects to `/hubs/notifications?access_token=<jwt>`.
- Recipients come from `INotificationRecipients` (system admins, a teacher's owners), never hard-coded ids.

## Subscriptions

- `SubscriptionRules` is the single place that decides a teacher's state: `Trial`, `Active`, `ExpiringSoon`, `InGrace`, `Expired`, `Suspended`. It has a C# version and a query version that must stay in step.
- Payments are cash, recorded by the system admin: a new paid period starts after the current one (or today, when coming from the trial).
- `SubscriptionReminderWorker` warns teachers and admins `ExpiryWarningDays` before the end, once per period.
- Settings: `Subscriptions:TrialDays`, `GraceDays`, `ExpiryWarningDays`.

## Code style

- Same as Tamam: file-scoped namespaces, one type per file, a blank line between statements and members, no unused usings (the build enforces it).
- Use `TimeProvider` for the current time, never `DateTime.UtcNow`, so tests can move the clock.
