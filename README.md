# FortuneForge.Games

`FortuneForge.Games` is the independently versioned home for Fortune Forge game code. It does not own the public API host, Firebase configuration, authentication, customer balances, payments, or the immutable account ledger.

## Boundary

- `FortuneForge.App` owns the website shell, API composition, identity, accounts, settlement authorization, admin, and deployments.
- `FortuneForge.Games` owns game rules, deterministic state transitions, public game contracts, game-specific client packages, assets, and focused tests.
- A game can calculate a settlement intent, but it cannot write a customer balance or receive unrestricted Firebase credentials.

The application consumes immutable, versioned NuGet packages from this repository. A vendored bootstrap feed keeps local and isolated-release builds reproducible until authenticated private-feed restore is connected in CI.

## Initial projects

- `FortuneForge.Games.Abstractions`: identifiers, descriptors, capabilities, and stable module contracts.
- `FortuneForge.Games.Cards`: standard playing-card primitives plus deterministic, account-neutral bot runtime support.
- `FortuneForge.Games.Blackjack`: Blackjack rules, bot decisions, table state, turn cadence, dealer flow, and round transitions.
- `FortuneForge.Games.Solitaire`: deterministic Klondike rules, bot decisions, competition state, ranking, and simulation.
- `FortuneForge.Games.TexasHoldem`: deck and hand rules, bot decisions, table state, blinds, turn progression, side pots, and showdown settlement intents.
- `FortuneForge.Games.Tests`: package contract and compatibility tests.

Game manifests under `catalog/games` are the source for the future generated website catalog. Adding a game should eventually require a package and manifest rather than edits to application navigation.

The application integration removes duplicated rules, state machines, and bot decision engines for Blackjack, Solitaire, and Texas Hold'em from the API repository. Matchmaking, persistence, HTTP projection, authentication, and actual money movement remain application adapters. Game packages can calculate deterministic settlement intent but cannot access account stores or credentials.

## Verify

```powershell
dotnet test FortuneForge.Games.slnx -c Release
```

See [docs/MIGRATION.md](docs/MIGRATION.md) for the staged extraction plan.
See [docs/PUBLISHING.md](docs/PUBLISHING.md) for the bootstrap and private-feed workflow.
