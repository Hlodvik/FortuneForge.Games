# FortuneForge.Games

`FortuneForge.Games` is the independently versioned home for Fortune Forge game code. It does not own the public API host, Firebase configuration, authentication, customer balances, payments, or the immutable account ledger.

## Boundary

- `FortuneForge.App` owns the website shell, API composition, identity, accounts, settlement authorization, admin, and deployments.
- `FortuneForge.Games` owns game rules, deterministic state transitions, public game contracts, game-specific client packages, assets, and focused tests.
- A game can calculate a settlement intent, but it cannot write a customer balance or receive unrestricted Firebase credentials.

The application consumes immutable, versioned NuGet packages from this repository. A vendored bootstrap feed keeps local and isolated-release builds reproducible until authenticated private-feed restore is connected in CI.

## Initial projects

- `FortuneForge.Games.Abstractions`: identifiers, descriptors, capabilities, and stable module contracts.
- `FortuneForge.Games.Cards`: standard playing-card primitives and the existing wire-code format.
- `FortuneForge.Games.Blackjack`: Blackjack actions, outcomes, hand scoring, and single-hand transitions.
- `FortuneForge.Games.Solitaire`: deterministic Klondike deal and move validation.
- `FortuneForge.Games.TexasHoldem`: deterministic deck generation and hand evaluation.
- `FortuneForge.Games.Tests`: package contract and compatibility tests.

Game manifests under `catalog/games` are the source for the future generated website catalog. Adding a game should eventually require a package and manifest rather than edits to application navigation.

The first application integration removes duplicated Blackjack rules, the full Solitaire rules engine, and Texas Hold'em deck/evaluation rules from the API repository. Multiplayer lobby orchestration, bots, persistence, HTTP DTO projection, and money movement remain application adapters until their platform dependencies are expressed as narrow interfaces.

## Verify

```powershell
dotnet test FortuneForge.Games.slnx -c Release
```

See [docs/MIGRATION.md](docs/MIGRATION.md) for the staged extraction plan.
See [docs/PUBLISHING.md](docs/PUBLISHING.md) for the bootstrap and private-feed workflow.
