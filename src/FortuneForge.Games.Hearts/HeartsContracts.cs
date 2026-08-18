using FortuneForge.Games.Cards;
using FortuneForge.Games.TrickTaking;

namespace FortuneForge.Games.Hearts;

public enum HeartsPhase
{
    Playing,
    Complete,
}

public enum HeartsEventType
{
    CardPlayed,
    TrickCompleted,
    RoundCompleted,
}

public sealed record HeartsScore(PlayerSeat Seat, int Points);

public sealed record HeartsState(
    uint Seed,
    PlayerSeat Turn,
    HeartsPhase Phase,
    IReadOnlyList<PlayerHand> Hands,
    TrickState CurrentTrick,
    IReadOnlyList<CompletedTrick> CompletedTricks,
    IReadOnlyList<HeartsScore> Scores,
    bool HeartsBroken)
{
    public IReadOnlyList<PlayingCard> HandFor(PlayerSeat seat) =>
        Hands.Single(hand => hand.Seat == seat).Cards;
}

public abstract record HeartsCommand(PlayerSeat Seat);

public sealed record PlayHeartsCard(PlayerSeat Seat, PlayingCard Card) : HeartsCommand(Seat);

public sealed record HeartsTransition(
    HeartsState State,
    HeartsEventType EventType,
    string Message);

public sealed class HeartsRuleException(string message) : InvalidOperationException(message);
