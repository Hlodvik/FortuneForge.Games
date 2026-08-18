using FortuneForge.Games.Abstractions;
using FortuneForge.Games.Roulette;

namespace FortuneForge.Games.Tests;

public sealed class RouletteEngineTests
{
    [Fact]
    public void StraightBetPaysThirtyFiveToOnePlusStake()
    {
        var bet = new RouletteBet("player", RouletteBetKind.Straight, 10m, 17);

        var result = RouletteEngine.Settle(bet, new RoulettePocket(17));

        Assert.True(result.Won);
        Assert.Equal(360m, result.TotalReturn);
    }

    [Theory]
    [InlineData(RouletteBetKind.Red)]
    [InlineData(RouletteBetKind.Black)]
    [InlineData(RouletteBetKind.Even)]
    [InlineData(RouletteBetKind.Odd)]
    [InlineData(RouletteBetKind.Low)]
    [InlineData(RouletteBetKind.High)]
    public void ZeroLosesEveryEvenMoneyBet(RouletteBetKind kind)
    {
        var result = RouletteEngine.Settle(
            new RouletteBet("player", kind, 5m),
            new RoulettePocket(0));

        Assert.False(result.Won);
        Assert.Equal(0m, result.TotalReturn);
    }

    [Fact]
    public void SpinSettlesAllBetsAndClosesRound()
    {
        var state = RouletteEngine.OpenRound("round-1");
        state = RouletteEngine.PlaceBet(state, new RouletteBet("alice", RouletteBetKind.Red, 5m));
        state = RouletteEngine.PlaceBet(state, new RouletteBet("bob", RouletteBetKind.Black, 5m));

        var spin = RouletteEngine.Spin(state, new RoulettePocket(1));

        Assert.Equal(RouletteRoundPhase.Settled, spin.State.Phase);
        Assert.Equal(10m, spin.Settlements.Single(result => result.PlayerId == "alice").TotalReturn);
        Assert.Equal(0m, spin.Settlements.Single(result => result.PlayerId == "bob").TotalReturn);
        Assert.Throws<RouletteRuleException>(() => RouletteEngine.Spin(spin.State, new RoulettePocket(2)));
    }

    [Fact]
    public void BetContractRejectsInvalidShapes()
    {
        var round = RouletteEngine.OpenRound("round-1");

        Assert.Throws<ArgumentOutOfRangeException>(() => new RoulettePocket(37));
        Assert.Throws<ArgumentException>(() => RouletteEngine.PlaceBet(
            round,
            new RouletteBet("player", RouletteBetKind.Straight, 5m)));
        Assert.Throws<ArgumentException>(() => RouletteEngine.PlaceBet(
            round,
            new RouletteBet("player", RouletteBetKind.Red, 5m, 12)));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouletteEngine.PlaceBet(
            round,
            new RouletteBet("player", (RouletteBetKind)999, 5m)));
    }

    [Fact]
    public void DescriptorDeclaresSingleZeroCasinoSkeleton()
    {
        var descriptor = RouletteModule.Descriptor;

        Assert.Equal("roulette", descriptor.Id);
        Assert.Equal("0.1.0", descriptor.PackageVersion);
        Assert.Equal(GameCategory.Casino, descriptor.Category);
        Assert.True(descriptor.Capabilities.HasFlag(GameCapability.FreePlay));
        Assert.False(descriptor.Capabilities.HasFlag(GameCapability.Credits));
        Assert.False(descriptor.Capabilities.HasFlag(GameCapability.History));
    }
}
