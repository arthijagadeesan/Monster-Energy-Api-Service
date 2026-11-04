# Monster Energy – Store Visits API

A .NET 10 Web API for the store list screen: search, filter, sort, paging, favourites and store details.

The seed file is a list of store visits (TDM, store, region, channel, date and start time), so the API is built around stores.

## Run it

```
cd backend
dotnet run --project MonsterEnergy.WebApi --launch-profile http
```

- Swagger UI: http://localhost:5236/swagger
- OpenAPI JSON: http://localhost:5236/openapi/v1.json

On startup the app applies migrations and loads `App_Data/stores-seed.json` into SQLite if the table is empty.

## Login

All store endpoints need a JWT.

1. `POST /api/auth/login` with `{ "username": "admin", "password": "Monster@123" }`
2. Copy `accessToken` and click **Authorize** in Swagger.

The token lasts 60 minutes (`Jwt:ExpiryMinutes` in `appsettings.json`).

## Endpoints

| Screen element | Endpoint |
|---|---|
| Login | `POST /api/auth/login` |
| Search, filter, sort, paging | `GET /api/stores?search=&region=&channel=&favouritesOnly=&sortBy=&sortDir=&page=&pageSize=` |
| Filter chips | `GET /api/stores/regions`, `GET /api/stores/channels` |
| View (eye) | `GET /api/stores/{storeGuid}` |
| Favourite (star) | `PUT /api/stores/{storeGuid}/favourite`, `DELETE /api/stores/{storeGuid}/favourite` |

- `sortBy`: `storeName`, `tdm`, `region`, `storeChannel`, `date`, `storeStart`
- `sortDir`: `asc` or `desc`
- `page` starts at 1, `pageSize` is 1–100
- The list returns `items`, `totalCount`, `page`, `pageSize` and `totalPages`.
- Errors come back as ProblemDetails (400 validation, 401, 404, 500).

## Tests

```
dotnet test
```

- Repository tests run against in-memory SQLite, so the real SQL is tested.
- API tests use `WebApplicationFactory` with a temporary database and cover auth, validation, paging, detail and favourites.

## Project layout

- **Domain** – entities (`Store`, `UserFavourite`). No dependencies.
- **Application** – DTOs, interfaces, `StoreService`.
- **Infrastructure** – EF Core, repository, seeding, JWT.
- **WebApi** – controllers, `Program.cs`.
- **Tests** – xUnit.


## Decisions and shortcuts

- **Filter chips.** The brief mentions a status filter, but the data has no status field. I used region and channel because they have a small, fixed set of values.
- **Favourites are per user.** They live in a `UserFavourites` table keyed by username and store, so one user's star doesn't show for everyone.
- **Users are in `appsettings.json`.** There's one demo user with a hashed password. A real app would use a users table or an identity provider.
- **The signing key is in `appsettings.json`.** That's only for this demo. In production it should come from user secrets, an environment variable or Key Vault.
- **No refresh tokens.** The client logs in again after an hour.
- **Search ignores case for English letters only.** SQLite `LIKE` doesn't fold Polish letters, so `łódź` won't match `Łódź`. A normalised search column or a different database would fix that.
- **A page past the end** returns empty `items` with the correct `totalCount`.

## What I'd add next

- Roles, for example a TDM only sees their own territory.
- Offline sync for the tablet app (ETags or "changed since" queries).
- Caching for the regions and channels lists.
- Health checks and structured logging/tracing.
- SQL Server or Postgres with full-text search.
