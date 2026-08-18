# Migration plan

## Rules of the extraction

1. Preserve the existing public API routes and serialized DTOs.
2. Move pure rules before persistence, HTTP, authentication, or accounting adapters.
3. Keep the Fortune Forge application as the only public API host and balance authority.
4. Publish immutable package versions; never reference a developer's sibling checkout in production.
5. Require compatibility tests before deleting the old implementation.

## Stages

### 1. Foundation — current milestone

- Create the independent solution and package conventions.
- Define game descriptors and catalog manifests.
- Extract shared card primitives.
- Port the pure Texas Hold'em hand evaluator as the first compatibility pilot.

The application is intentionally unchanged during this stage.

### 2. Package integration

- Configure a private NuGet feed and npm registry.
- Publish prerelease packages from tagged builds.
- Update the API host to consume `FortuneForge.Games.TexasHoldem` through a thin compatibility adapter.
- Compare the package evaluator against the existing evaluator with the full canonical and randomized corpus.
- Remove the old evaluator only after the package-backed application tests pass.

### 3. Game-by-game server extraction

- Texas Hold'em state machine and bots.
- Blackjack rules and table state machine.
- Solitaire engine and validation.
- Slot definitions, math, and feature engines.

Firestore stores, controllers, rate limits, authentication, and account settlement stay in `FortuneForge.App` until explicit platform interfaces exist.

### 4. Client packages and generated catalog

- Add a versioned TypeScript game SDK.
- Move each game's client transport, UI, styles, and assets into its package.
- Generate lazy routes and catalog cards from manifests.
- Keep account navigation and the shared website shell in the application repository.

### 5. Scale-out only when justified

The initial runtime remains a modular API host. A game becomes a separately deployed service only when load, ownership, or release cadence proves that the additional network and operational boundary is worthwhile.
