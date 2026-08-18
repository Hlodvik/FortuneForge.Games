using FortuneForge.Games.Abstractions;
using FortuneForge.Games.Cards;
using FortuneForge.Games.Spades;
using FortuneForge.Games.TrickTaking;

namespace FortuneForge.Games.Tests;

public sealed class SpadesEngineTests
{
    [Fact]
    public void FourClockwiseBidsOpenThePlayingPhase()
    {
        var state = SpadesEngine.Start(7);

        foreach (var seat in new[] { PlayerSeat.North, PlayerSeat.East, PlayerSeat.South, PlayerSeat.West })
            state = SpadesEngine.Apply(state, new PlaceSpadesBid(seat, 3)).State;

        Assert.Equal(SpadesPhase.Playing, state.Phase);
        Assert.Equal(PlayerSeat.North, state.Turn);
        Assert.Equal(4, state.Bids.Count);
    }

    [Fact]
    public void SpadesCannotLeadBeforeTheyAreBrokenWhenAnotherSuitExists()
    {
        var state = PlayingState(
            [Card(CardRank.Ace, CardSuit.Spades), Card(CardRank.Two, CardSuit.Clubs)],
            [],
            [],
            []);

        var exception = Assert.Throws<SpadesRuleException>(() => SpadesEngine.Apply(
            state,
            new PlaySpadesCard(PlayerSeat.North, Card(CardRank.Ace, CardSuit.Spades))));

        Assert.Contains("broken", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BasicScoringRejectsUnsupportedNilBids()
    {
        var state = SpadesEngine.Start(7);

        var exception = Assert.Throws<SpadesRuleException>(() =>
            SpadesEngine.Apply(state, new PlaceSpadesBid(PlayerSeat.North, 0)));

        Assert.Contains("one through thirteen", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TrumpTakesTheTrickAndLeadsNext()
    {
        var state = PlayingState(
            [Card(CardRank.Ace, CardSuit.Clubs)],
            [Card(CardRank.Two, CardSuit.Spades)],
            [Card(CardRank.King, CardSuit.Clubs)],
            [Card(CardRank.Queen, CardSuit.Clubs)]);
        state = Play(state, PlayerSeat.North, Card(CardRank.Ace, CardSuit.Clubs));
        state = Play(state, PlayerSeat.East, Card(CardRank.Two, CardSuit.Spades));
        state = Play(state, PlayerSeat.South, Card(CardRank.King, CardSuit.Clubs));

        var result = SpadesEngine.Apply(
            state,
            new PlaySpadesCard(PlayerSeat.West, Card(CardRank.Queen, CardSuit.Clubs)));

        Assert.Equal(SpadesEventType.TrickCompleted, result.EventType);
        Assert.Equal(PlayerSeat.East, result.State.Turn);
        Assert.Equal(1, result.State.TricksWon.Single(count => count.Seat == PlayerSeat.East).Tricks);
        Assert.True(result.State.SpadesBroken);
    }

    [Fact]
    public void FinalTrickProducesBasicPartnershipScore()
    {
        var bids = Enum.GetValues<PlayerSeat>().Select(seat => new SpadesBid(seat, 3)).ToArray();
        var tricks = Enum.GetValues<PlayerSeat>().Select(seat => new SpadesTrickCount(seat, 3)).ToArray();
        var state = new SpadesState(
            1,
            PlayerSeat.West,
            PlayerSeat.West,
            SpadesPhase.Playing,
            Hands([], [], [], [Card(CardRank.Jack, CardSuit.Clubs)]),
            bids,
            new TrickState(PlayerSeat.North,
            [
                new(PlayerSeat.North, Card(CardRank.Ace, CardSuit.Clubs)),
                new(PlayerSeat.East, Card(CardRank.King, CardSuit.Clubs)),
                new(PlayerSeat.South, Card(CardRank.Queen, CardSuit.Clubs)),
            ]),
            Enumerable.Range(1, 12)
                .Select(number => new CompletedTrick(number, PlayerSeat.North, PlayerSeat.North, []))
                .ToArray(),
            tricks,
            false,
            null);

        var result = SpadesEngine.Apply(
            state,
            new PlaySpadesCard(PlayerSeat.West, Card(CardRank.Jack, CardSuit.Clubs)));

        Assert.Equal(SpadesPhase.Complete, result.State.Phase);
        Assert.Equal(new SpadesTeamScore(61, 60), result.State.Score);
    }

    [Fact]
    public void DescriptorIdentifiesThePreviewShellContract()
    {
        Assert.Equal("Spades", SpadesModule.Descriptor.DisplayName);
        Assert.Equal("0.1.0", SpadesModule.Descriptor.PackageVersion);
        Assert.Equal("/cards/spades", SpadesModule.Descriptor.ClientRoute);
        Assert.Equal("/api/games/spades", SpadesModule.Descriptor.ApiBasePath);
        Assert.False(SpadesModule.Descriptor.Capabilities.HasFlag(GameCapability.Credits));
    }

    private static SpadesState Play(SpadesState state, PlayerSeat seat, PlayingCard card) =>
        SpadesEngine.Apply(state, new PlaySpadesCard(seat, card)).State;

    private static SpadesState PlayingState(
        IReadOnlyList<PlayingCard> north,
        IReadOnlyList<PlayingCard> east,
        IReadOnlyList<PlayingCard> south,
        IReadOnlyList<PlayingCard> west) => new(
        1,
        PlayerSeat.West,
        PlayerSeat.North,
        SpadesPhase.Playing,
        Hands(north, east, south, west),
        Enum.GetValues<PlayerSeat>().Select(seat => new SpadesBid(seat, 1)).ToArray(),
        new TrickState(PlayerSeat.North, []),
        [],
        Enum.GetValues<PlayerSeat>().Select(seat => new SpadesTrickCount(seat, 0)).ToArray(),
        false,
        null);

    private static IReadOnlyList<PlayerHand> Hands(params IReadOnlyList<PlayingCard>[] hands) =>
        Enum.GetValues<PlayerSeat>().Select((seat, index) => new PlayerHand(seat, hands[index])).ToArray();

    private static PlayingCard Card(CardRank rank, CardSuit suit) => new(rank, suit);
}
