using FortuneForge.Games.Cards;
using FortuneForge.Games.TrickTaking;

namespace FortuneForge.Games.Spades;

public enum SpadesPhase
{
    Bidding,
    Playing,
    Complete,
}

public enum SpadesEventType
{
    BidPlaced,
    CardPlayed,
    TrickCompleted,
    RoundCompleted,
}

public sealed record SpadesBid(PlayerSeat Seat, int Tricks);

public sealed record SpadesTrickCount(PlayerSeat Seat, int Tricks);

public sealed record SpadesTeamScore(int NorthSouth, int EastWest);

public sealed record SpadesState(
    uint Seed,
    PlayerSeat Dealer,
    PlayerSeat Turn,
    SpadesPhase Phase,
    IReadOnlyList<PlayerHand> Hands,
    IReadOnlyList<SpadesBid> Bids,
    TrickState CurrentTrick,
    IReadOnlyList<CompletedTrick> CompletedTricks,
    IReadOnlyList<SpadesTrickCount> TricksWon,
    bool SpadesBroken,
    SpadesTeamScore? Score)
{
    public IReadOnlyList<PlayingCard> HandFor(PlayerSeat seat) =>
        Hands.Single(hand => hand.Seat == seat).Cards;
}

public abstract record SpadesCommand(PlayerSeat Seat);

public sealed record PlaceSpadesBid(PlayerSeat Seat, int Tricks) : SpadesCommand(Seat);

public sealed record PlaySpadesCard(PlayerSeat Seat, PlayingCard Card) : SpadesCommand(Seat);

public sealed record SpadesTransition(
    SpadesState State,
    SpadesEventType EventType,
    string Message);

public sealed class SpadesRuleException(string message) : InvalidOperationException(message);
