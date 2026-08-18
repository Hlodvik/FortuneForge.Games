using FortuneForge.Games.Abstractions;

namespace FortuneForge.Games.Spades;

public static class SpadesModule
{
    public static GameDescriptor Descriptor { get; } = CreateDescriptor();

    private static GameDescriptor CreateDescriptor()
    {
        var descriptor = new GameDescriptor(
            "spades",
            "Spades",
            GameCategory.Card,
            "0.1.0",
            "/cards/spades",
            "/api/games/spades",
            GameCapability.FreePlay | GameCapability.Multiplayer);
        descriptor.Validate();
        return descriptor;
    }
}
