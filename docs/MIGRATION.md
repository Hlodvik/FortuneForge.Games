# Migration plan

## Rules of the extraction

1. Preserve the existing public API routes and serialized DTOs.
2. Move pure rules before persistence, HTTP, authentication, or accounting adapters.
3. Keep the Fortune Forge application as the only public API host and balance authority.
4. Publish immutable package versions; never reference a developer's sibling checkout in production.
5. Require compatibility tests before deleting the old implementation.

## Stages

### 1. Foundation — complete

- Create the independent solution and package conventions.
- Define game descriptors and catalog manifests.
- Extract shared card primitives.
- Port the pure Texas Hold'em hand evaluator as the first compatibility pilot.

The application is intentionally unchanged during this stage.

### 2. Package integration — complete for the first three card games

- Configure private NuGet publishing and keep a vendored bootstrap feed until authenticated CI restore is available.
- Publish prerelease packages from tagged builds.
- Update the API host to consume the Hold'em, Blackjack, and Solitaire rules packages through compatibility imports.
- Compare package rules against the existing behavior with canonical and application-level tests.
- Remove duplicated Blackjack, Solitaire, and Hold'em rule implementations only after package-backed application tests pass.

### 3. Game-by-game server extraction

- Texas Hold'em multiplayer state machine and bots.
- Blackjack multiplayer table orchestration.
- Solitaire tournament coordination.
- Slot definitions, math, and feature engines.

Firestore stores, controllers, rate limits, authentication, and account settlement stay in `FortuneForge.App` until explicit platform interfaces exist.

### 4. Client packages and generated catalog

- Add a versioned TypeScript game SDK.
- Move each game's client transport, UI, styles, and assets into its package.
- Generate lazy routes and catalog cards from manifests.
- Keep account navigation and the shared website shell in the application repository.

### 5. Scale-out only when justified

The initial runtime remains a modular API host. A game becomes a separately deployed service only when load, ownership, or release cadence proves that the additional network and operational boundary is worthwhile.
