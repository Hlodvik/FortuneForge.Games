using System.Text.Json;
using FortuneForge.Games.Abstractions;

namespace FortuneForge.Games.Tests;

public sealed class GameCatalogManifestTests
{
    [Fact]
    public void EveryCatalogManifestMapsToAValidDescriptor()
    {
        var root = FindRepositoryRoot();
        var paths = Directory.GetFiles(Path.Combine(root, "catalog", "games"), "*.game.json");

        Assert.NotEmpty(paths);
        foreach (var path in paths)
        {
            var manifest = JsonSerializer.Deserialize<CatalogManifest>(
                File.ReadAllText(path),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(manifest);
            Assert.Equal(1, manifest.SchemaVersion);
            var descriptor = new GameDescriptor(
                manifest.Id,
                manifest.DisplayName,
                ParseCategory(manifest.Category),
                manifest.PackageVersion,
                manifest.ClientRoute,
                manifest.ApiBasePath,
                ParseCapabilities(manifest.Capabilities));
            descriptor.Validate();
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "FortuneForge.Games.slnx")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("The FortuneForge.Games repository root was not found.");
    }

    private static GameCategory ParseCategory(string category) => category switch
    {
        "card" => GameCategory.Card,
        "slot" => GameCategory.Slot,
        "casino" => GameCategory.Casino,
        "arcade" => GameCategory.Arcade,
        "dice" => GameCategory.Dice,
        "other" => GameCategory.Other,
        _ => throw new InvalidDataException($"Unknown game category '{category}'."),
    };

    private static GameCapability ParseCapabilities(IReadOnlyList<string> capabilities) =>
        capabilities.Aggregate(GameCapability.None, (result, capability) => result | (capability switch
        {
            "credits" => GameCapability.Credits,
            "free-play" => GameCapability.FreePlay,
            "multiplayer" => GameCapability.Multiplayer,
            "bots" => GameCapability.Bots,
            "history" => GameCapability.History,
            _ => throw new InvalidDataException($"Unknown game capability '{capability}'."),
        }));

    private sealed record CatalogManifest(
        int SchemaVersion,
        string Id,
        string DisplayName,
        string Category,
        string PackageVersion,
        string ClientRoute,
        string? ApiBasePath,
        IReadOnlyList<string> Capabilities);
}
