# FortuneForge.Games

`FortuneForge.Games` is the independently versioned home for Fortune Forge game code. It does not own the public API host, Firebase configuration, authentication, customer balances, payments, or the immutable account ledger.

## Boundary

- `FortuneForge.App` owns the website shell, API composition, identity, accounts, settlement authorization, admin, and deployments.
- `FortuneForge.Games` owns game rules, deterministic state transitions, public game contracts, game-specific client packages, assets, and focused tests.
- A game can calculate a settlement intent, but it cannot write a customer balance or receive unrestricted Firebase credentials.

The application will consume immutable, versioned NuGet and npm packages from this repository. Until package publishing is configured, extracted implementations remain duplicated behind compatibility tests and are not referenced by the live application.

## Initial projects

- `FortuneForge.Games.Abstractions`: identifiers, descriptors, capabilities, and stable module contracts.
- `FortuneForge.Games.Cards`: standard playing-card primitives and the existing wire-code format.
- `FortuneForge.Games.TexasHoldem`: the first pure rules extraction, beginning with hand evaluation.
- `FortuneForge.Games.Tests`: package contract and compatibility tests.

Game manifests under `catalog/games` are the source for the future generated website catalog. Adding a game should eventually require a package and manifest rather than edits to application navigation.

## Verify

```powershell
dotnet test FortuneForge.Games.slnx -c Release
```

See [docs/MIGRATION.md](docs/MIGRATION.md) for the staged extraction plan.
