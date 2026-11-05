# Manbaa API

Every response has the same shape:

```json
{ "succeeded": true, "statusCode": 200, "message": null, "errors": [], "data": { } }
```

Send the token as `Authorization: Bearer <token>`. Enums are strings. Dates are UTC.
Paged lists take `page`, `pageSize` (max 100) and `search`, and return `{ items, page, pageSize, totalCount, totalPages, hasNext, hasPrevious }`.

## Auth

| Method | Path | Who | Notes |
|---|---|---|---|
| POST | `/api/auth/login` | anyone | `{ login, password }` — `login` is the email or the phone. Returns `{ token, expiresAt, userId, teacherId, fullName, role }`. |
| GET | `/api/auth/me` | signed in | The current user from the token. |

Roles: `SystemAdmin`, `Teacher`, `Secretary`, `Student`, `Parent`.

A teacher whose subscription ended (after the grace days) or who is suspended gets `401` with the message
"اشتراك الحساب ده منتهي أو موقوف..." on every request, together with all of the teacher's users.

## Notifications (any signed-in user)

| Method | Path | Notes |
|---|---|---|
| GET | `/api/notifications?unreadOnly=true` | Paged, newest first. |
| GET | `/api/notifications/unread-count` | `data` is a number. |
| POST | `/api/notifications/{id}/read` | |
| POST | `/api/notifications/read-all` | |

Live: connect SignalR to `/hubs/notifications?access_token=<token>` and listen for `notification`
(payload: `{ id, type, title, body, link, isRead, createdAt }`).

Types so far: `SubscriptionRenewed`, `SubscriptionCancelled`, `SubscriptionExpiringSoon`, `AccountSuspended`, `AccountActivated`, `TeacherJoined`.

## Teacher

| Method | Path | Notes |
|---|---|---|
| GET | `/api/subscription` | The teacher's plan, limits, state, end date, days left and payment history. |
| GET | `/api/audit-logs` | The teacher's activity log. Filters: `userId`, `entityName`, `entityId`, `action`, `from`, `to`, `search`. |

## System admin (`/api/admin`)

### Dashboard

`GET /api/admin/dashboard` returns:
`totalTeachers, trialTeachers, activeTeachers, expiringSoonTeachers, inGraceTeachers, expiredTeachers, suspendedTeachers, newTeachersThisMonth, revenueThisMonth, revenueLastMonth, revenueByMonth[12] { year, month, amount, payments }, expiringSoon[10]`.

### Teachers

| Method | Path | Notes |
|---|---|---|
| GET | `/api/admin/teachers` | Paged. Filters: `search` (name, subject, phone, email — Arabic-aware), `state` (`Trial`, `Active`, `ExpiringSoon`, `InGrace`, `Expired`, `Suspended`), `planId`. |
| GET | `/api/admin/teachers/{id}` | Details with the payment history. |
| POST | `/api/admin/teachers` | `{ fullName, subject, phone, email?, password, planId?, notes? }` — creates the teacher and their login, starts the free trial. |
| PUT | `/api/admin/teachers/{id}` | `{ fullName, subject, phone, email?, notes? }` |
| POST | `/api/admin/teachers/{id}/suspend` | `{ reason }` — locks out the teacher and all of their users. |
| POST | `/api/admin/teachers/{id}/activate` | |
| POST | `/api/admin/teachers/{id}/subscriptions` | Record a cash payment: `{ planId, months (1–24), amount?, notes? }`. `amount` defaults to plan price × months. Returns the receipt (`receiptNumber` like `S-000012`). |

### Subscriptions (payments)

| Method | Path | Notes |
|---|---|---|
| GET | `/api/admin/subscriptions` | Paged. Filters: `teacherId`, `from`, `to`, `includeCancelled`, `search`. |
| POST | `/api/admin/subscriptions/{id}/cancel` | `{ reason }` — only the teacher's latest payment; the record is kept. |

### Plans

| Method | Path | Notes |
|---|---|---|
| GET | `/api/admin/plans?activeOnly=true` | With `teachersCount`. |
| GET | `/api/admin/plans/{id}` | |
| POST | `/api/admin/plans` | `{ name, description?, monthlyPrice, maxStudents, maxSecretaries, maxStorageGb, isActive, sortOrder }` |
| PUT | `/api/admin/plans/{id}` | same body |
| DELETE | `/api/admin/plans/{id}` | Refused while teachers use the plan (deactivate it instead). |

### Audit log

`GET /api/admin/audit-logs` — the whole system. Same filters as the teacher's, plus `teacherId`.
Each row: `{ id, teacherId, userId, userName, userRole, action, entityName, entityId, oldValues, newValues, ipAddress, userAgent, createdAt }`;
`action` is `Create`, `Update`, `Delete`, `Login` or `LoginFailed`; `oldValues`/`newValues` are JSON of the changed fields.
