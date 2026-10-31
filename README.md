# ShiftStudy

A study planner for people who work rostered shifts. It reads your roster, finds the time you actually have, plans study around your shifts and your energy, and replans automatically when the roster or your week changes.

## Why

I work full-time as a store manager on a rotating roster while doing a Professional Year program, the 12-course Microsoft Full-Stack Developer certificate and English test preparation. Every planner I tried assumed a fixed 9-to-5 week. My roster changes weekly, a free evening after a close shift is not the same as a free morning on a day off, and one changed shift breaks the whole week's plan. ShiftStudy plans my own study, including the certificate it is built in.

## Status

Course 1: solution structure, CI and architecture tests in place. The scheduling engine is next (Course 2).

## Solution structure

| Project | Purpose |
|---|---|
| `src/ShiftStudy.Domain` | Entities, value objects, business rules |
| `src/ShiftStudy.Scheduling` | The scheduling engine: intervals, energy rules, allocation, forecast. Depends only on the BCL and NodaTime |
| `src/ShiftStudy.Application` | Feature folders (Roster, Goals, Planning, Sessions, Reviews): services, DTOs, validators, interfaces |
| `src/ShiftStudy.Infrastructure` | EF Core, cache, blob storage, background jobs, AI and push integrations |
| `src/ShiftStudy.Api` | Controllers, SignalR hub, auth, health checks, iCal feed (a library hosted by Web) |
| `src/ShiftStudy.Web` | Blazor Web App host; also hosts the API |
| `src/ShiftStudy.Web.Client` | Blazor WebAssembly components, PWA, offline storage |
| `tests/*` | Scheduling (unit, property, DST), benchmarks, application, API integration, bUnit components, architecture rules |
| `tests/load` | k6 scripts |
| `deploy` | Bicep templates |

Dependencies point inward: Api and Infrastructure → Application → Domain and Scheduling. `ShiftStudy.ArchitectureTests` enforces this on every build.

## Running locally

Requires the .NET 10 SDK.

```bash
dotnet build
dotnet test
dotnet run --project src/ShiftStudy.Web
```

`docker-compose.yml` starts SQL Server, Redis and Azurite. It is not needed until the database work in Course 6.

## Design decisions

See [DECISIONS.md](DECISIONS.md).
