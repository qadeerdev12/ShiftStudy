# Architecture decision records

Each record states the decision, why it was made, and what it changes from the original specification.

## ADR-001: Modular monolith with a framework-free scheduling engine

**Date:** 2026-10-31 · **Status:** Accepted

**Decision.** One deployable app with clear modules. The scheduling engine is its own project (`ShiftStudy.Scheduling`) that depends only on the BCL and NodaTime. It does not reference Domain entities: it defines its own input records and Application maps to them.

**Why.** One developer, one deployment and one transaction per replan. A pure engine is fast, deterministic and fully unit-testable.

## ADR-002: Engine entry point takes the current time as input

**Date:** 2026-10-31 · **Status:** Accepted

**Decision.** `PlanningEngine.Plan(PlanningRequest)` is the single entry point. The request carries `Instant Now`, the user's `DateTimeZone` and the `LocalDate WeekStart`, plus shifts, commitments, settings, rules, goals and existing blocks.

**Why.** Step 4 keeps blocks in the past, so the engine needs "now". Passing it in keeps the engine deterministic (NFR-3) and testable with any date, including the DST weekends.

## ADR-003: Deadline forecast for weeks without a known roster

**Date:** 2026-10-31 · **Status:** Accepted

**Decision.**
- Capacity for weeks beyond the known roster is the average usable Deep and Light minutes of the last 4 real weeks, multiplied by the completion rate (minutes done ÷ minutes planned). Until 4 weeks of history exist, the latest known week is repeated.
- Goals compete for the same hours, so risk is checked earliest-deadline-first: sort goals by deadline; for each deadline, the remaining minutes of all goals due by then must fit the capacity before it. Deep demand is checked against Deep capacity separately.
- Slack below 0 is **at risk** (the deficit is the hour shortfall), 0–20% is **tight**, otherwise **on track**.

**Why.** The spec's urgency and early-warning goal (2 weeks ahead) depend on time beyond the one or two weeks of roster you actually know. The completion rate keeps the forecast honest about how much of a plan really gets done.

**Changes from spec.** Sections 7.1 step 5 and step 7.

## ADR-004: Study blocks keep a stable identity across replans

**Date:** 2026-10-31 · **Status:** Accepted

**Decision.** Blocks are stable rows per user and week. A replan updates, inserts or deletes Planned blocks in place and never touches fixed ones (locked, started, done, past). `Plan` becomes a version record (version, inputs hash, generated at), and a `BlockChange` table records each move for "what moved". The engine reuses existing Planned block IDs for the same task, so a move is reported as a move.

**Why.** Copying blocks into each new plan version would leave sessions, RowVersions and offline outbox replays pointing at old rows.

**Changes from spec.** Sections 6 (StudyBlock, Plan) and 7.2 (safe write).

## ADR-005: Every user-owned table carries UserId

**Date:** 2026-10-31 · **Status:** Accepted

**Decision.** Every user-owned entity implements `IUserOwned`, including `StudyTask`. The `DbContext` applies the `UserId` query filter to all of them in one loop, and a test fails if any `IUserOwned` entity has no filter.

**Why.** Per-user isolation (NFR-10) should not depend on remembering to add a filter for each new table.

**Changes from spec.** Section 6 (StudyTask).

## ADR-006: Debounced replans and polled reminders

**Date:** 2026-10-31 · **Status:** Accepted

**Decision.**
- *Debounce:* each change upserts a `ReplanRequest(UserId, WeekStart, RequestedAt)` row and queues a Hangfire job delayed by 5 seconds that carries its own `RequestedAt`. The job exits if the row holds a newer value, so only the last job in a burst replans.
- *Reminders:* one recurring job runs every minute, finds Planned blocks where `StartUtc − offset ≤ now` and `ReminderSentAt` is null, sends them, and stamps `ReminderSentAt`.

**Why.** Hangfire has no built-in debounce. Polling for reminders removes the need to cancel and reschedule a job every time a block moves, and meets NFR-7 (within 1 minute) by construction.

**Changes from spec.** Sections 7.2 (debounce) and 7.4 (reminders).

## ADR-007: Interactive WebAssembly with cookie auth; JWT as a second scheme

**Date:** 2026-10-31 · **Status:** Accepted

**Decision.** The Blazor app uses Interactive WebAssembly render mode and same-host cookie authentication. JWT bearer auth is added as a second scheme for non-browser clients (load tests, a future MAUI app). The SignalR Redis backplane is added with Redis in Course 9.

**Why.** An access token kept in memory is lost when the PWA reloads offline, and keeping refresh tokens in IndexedDB is an XSS risk. Offline mode needs the Today view running in WebAssembly anyway. Cookies on the same host need no token handling in the browser.

**Changes from spec.** Sections 4 (front end, auth) and 11 (tokens).

## ADR-008: Task estimates are the source of truth for goal hours

**Date:** 2026-10-31 · **Status:** Accepted

**Decision.** A goal's estimate is the sum of its tasks' estimates. A goal created with only a total gets one "Unplanned work" placeholder task that can be split later.

**Why.** One source of truth avoids the goal total and the task breakdown disagreeing.

## ADR-009: API hosted inside the Blazor Web App

**Date:** 2026-10-31 · **Status:** Accepted

**Decision.** `ShiftStudy.Api` is a class library of controllers and endpoints, mapped into the `ShiftStudy.Web` host. There is one App Service, not separate API and Web services.

**Why.** Halves hosting cost and deployment work, and same-host cookies (ADR-007) need it. The API can move to its own host later by adding a small `Program.cs`.

**Changes from spec.** Section 4 hosting, which lists separate API and Web App Services.

## ADR-010: AwesomeAssertions instead of FluentAssertions

**Date:** 2026-10-31 · **Status:** Accepted

**Decision.** Tests use AwesomeAssertions.

**Why.** FluentAssertions 8 needs a paid licence for commercial use. AwesomeAssertions is a community fork under the Apache 2.0 licence with the same API.
