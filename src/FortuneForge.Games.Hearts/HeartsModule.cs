using FortuneForge.Games.Abstractions;

namespace FortuneForge.Games.Hearts;

public static class HeartsModule
{
    public static GameDescriptor Descriptor { get; } = CreateDescriptor();

    private static GameDescriptor CreateDescriptor()
    {
        var descriptor = new GameDescriptor(
            "hearts",
            "Hearts",
            GameCategory.Card,
            "0.1.0",
            "/cards/hearts",
            "/api/games/hearts",
            GameCapability.FreePlay | GameCapability.Multiplayer);
        descriptor.Validate();
        return descriptor;
    }
}
