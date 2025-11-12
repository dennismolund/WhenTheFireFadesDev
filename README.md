# When the Fire Fades

When the Fire Fades is a web-based social deduction game for exactly five players. Two hidden shapeshifters try to snuff out the campfire while three humans fight to keep it alive by completing missions across five escalating rounds. The solution is organised according to Clean Architecture: the `Domain` project owns the core game rules, `Application` coordinates use cases, `Infrastructure` hosts technical concerns such as persistence and SignalR hubs, and the `Web` project delivers the ASP.NET Core MVC experience.

Game website: https://whenthefirefades-hqeqgxhph8cpgde6.eastasia-01.azurewebsites.net/

## Table of contents
- [Getting started](#getting-started)
- [Game overview](#game-overview)
- [Gameplay at a glance](#gameplay-at-a-glance)
- [Tech stack](#tech-stack)
- [Solution architecture](#solution-architecture)
- [Domain & data model](#domain--data-model)
- [Client experience](#client-experience)

## Getting started
1. **Clone & restore dependencies**
   ```bash
   git clone https://github.com/<your-account>/WhenTheFireFades.git
   cd WhenTheFireFades
   dotnet restore
   ```
2. **Configure the connection string**
   - Provide a `DefaultConnection` entry either in `appsettings.Development.json`, via [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets), or environment variables. The app targets SQL Server.
3. **Apply migrations**
   ```bash
   dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project Web/Web.csproj
   ```
4. **Run the app**
   ```bash
   dotnet run --project Web/Web.csproj
   ```
5. **Open the site**
   - Navigate to `https://localhost:5001` (or the HTTP port shown in console output).

## Game overview
- **Players:** 5 (3 Humans, 2 Shapeshifters)
- **Goal for Humans:** keep the fire burning by succeeding in 3 missions.
- **Goal for Shapeshifters:** sabotage 3 missions before the fire fades.
- **Setting:** a dark forest campfire where all discussion, accusations, and plotting happen between missions.

### Roles
| Role | Count | Capabilities |
| --- | --- | --- |
| Human teenagers | 3 | Must always choose to gather firewood during a mission.
| Shapeshifters | 2 | Choose between gathering wood or secretly sabotaging the mission.

### Round structure
Each round rotates leadership, proposes a mission team, and resolves the mission outcome.

1. **Leader assignment** – Leadership rotates seats 1 through 5.
2. **Team selection** – The leader proposes a mission team that matches the required team size for the round (2 or 3 players depending on the round).
3. **Team vote** – All players vote Yes/No. Majority approval sends the team; otherwise, leadership passes and the round restarts with the next player.
4. **Mission vote** – Mission team members secretly choose their action (Humans must gather wood, Shapeshifters may sabotage).
5. **Outcome** – The round succeeds only if every vote was gather wood; a single sabotage ends in failure.
6. **Victory checks** – The first side to accumulate 3 successes or 3 sabotages wins.

## Gameplay at a glance
| Round | Team size |
| --- | --- |
| 1 | 2 players |
| 2 | 3 players |
| 3 | 2 players |
| 4 | 3 players |
| 5 | 3 players |

Between missions, the lobby chat (delivered through SignalR) becomes the arena for bluffing, deduction, and strategising.

## Tech stack
- **ASP.NET Core 8 MVC** for the web application shell and Razor-based UI.
- **Entity Framework Core (SQL Server)** for persistence via the `ApplicationDbContext` and migrations.
- **SignalR** for pushing live lobby updates (ready states, joins/leaves) through `GameLobbyHub`.
- **Bootstrap 5** (via LibMan) for styling the lobby and landing page components.

## Solution architecture
The solution adheres to Clean Architecture with four isolated projects that reference one another from the inside out:

| Project | Layer | Responsibility |
| --- | --- | --- |
| `Domain/` | Core | Pure game domain model: entities (`Game`, `Round`, `Team`, etc.), business rules, value objects and supporting enums with no external dependencies. |
| `Application/` | Application | Use-case orchestration (`Services/GameOrchestrator`) and contracts (`Interfaces/IGameRepository`, `ITeamRepository`, etc.) |
| `Infrastructure/` | Infrastructure | EF Core persistence (`Persistence/ApplicationDbContext`, repository implementations, migrations). Implements the interfaces defined by the Application layer. |
| `Web/` | Presentation | ASP.NET Core MVC front end: controllers, hubs, Razor views, and DI configuration (`Program.cs`) that stitches Application and Infrastructure together.

## Domain & data model
The core entities are:

- **Game** – Tracks connection code, leader seat, round counters, win state etc.
- **GamePlayer** – Associates a seat, nickname, readiness, and temporary user identifier with a game.
- **Round** – Captures per-round metadata: leader, team size, phase progression, sabotage count, and team vote counter.
- **Team** – The selected team for a round, having one to many relations with the models below (TeamMember, MissionVote).
- **TeamMember** & **TeamVote** – Which seats were selected to partake in the team and how each player voted on the selected team.
- **MissionVote** – Secret votes cast by mission participants indicating gather wood or sabotage.

Refer to the `Domain/Entities/` folder for the exact property sets and data annotations that govern persistence rules. Entity Framework Core configuration and migrations live under `Infrastructure/Persistence/`.

## Client experience
- The landing page (`Views/Home/Index.cshtml`) presents options to create a game or join via code, showing the player nickname stored in session.
- Entering the lobby subscribes the browser to `GameLobbyHub` where join/leave and ready state changes update all participants instantly.
- Bootstrap cards and modals provide a polished UI for joining games while keeping the theme atmospheric and minimalistic.

---

Created and maintained by a solo developer—contributions or feedback are welcome via issues and pull requests.
