# Package publishing

## Bootstrap feed

Until the repository has a GitHub remote and package access is configured for every build identity, the application vendors approved `.nupkg` files under `packages/games`. This is a package cache, not a source-code dependency on a sibling checkout.

To refresh the bootstrap feed from a clean game repository checkout:

```powershell
dotnet test FortuneForge.Games.slnx -c Release
dotnet pack FortuneForge.Games.slnx -c Release --no-build --output <FortuneForge.App>/packages/games
```

Package versions are immutable. Change `VersionPrefix` before changing any package that has already been distributed.

## Private GitHub Packages feed

The `publish-packages` workflow publishes tags matching `games-v*` to the repository owner's private NuGet feed. After the remote exists:

1. Grant the FortuneForge application workflow read access to the game packages.
2. Add the authenticated GitHub Packages source in application CI.
3. Keep `nuget.org` for third-party packages.
4. Remove the vendored bootstrap packages only after clean CI and deployment builds can restore from the private feed.

The game repository never receives Firebase deployment credentials, account-store credentials, or payment-provider secrets.
