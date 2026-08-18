using System.Collections.Immutable;

namespace FortuneForge.Games.Roulette;

public static class RouletteEngine
{
    public static RouletteRoundState OpenRound(string roundId)
    {
        if (string.IsNullOrWhiteSpace(roundId))
            throw new ArgumentException("A round ID is required.", nameof(roundId));

        return new RouletteRoundState(
            roundId,
            RouletteRoundPhase.Open,
            [],
            null,
            []);
    }

    public static RouletteRoundState PlaceBet(RouletteRoundState state, RouletteBet bet)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(bet);
        if (state.Phase != RouletteRoundPhase.Open)
            throw new RouletteRuleException("Bets cannot be placed after the wheel has spun.");

        ValidateBet(bet);
        return state with { Bets = state.Bets.Add(bet) };
    }

    public static RouletteSpinOutcome Spin(RouletteRoundState state, RoulettePocket winningPocket)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (state.Phase != RouletteRoundPhase.Open)
            throw new RouletteRuleException("The round has already been settled.");
        if (state.Bets.IsDefaultOrEmpty)
            throw new RouletteRuleException("At least one bet is required before spinning.");

        var settlements = state.Bets
            .Select(bet => Settle(bet, winningPocket))
            .ToImmutableArray();
        var settled = state with
        {
            Phase = RouletteRoundPhase.Settled,
            WinningPocket = winningPocket,
            Settlements = settlements,
        };
        return new RouletteSpinOutcome(settled, winningPocket, settlements);
    }

    public static RouletteSettlement Settle(RouletteBet bet, RoulettePocket winningPocket)
    {
        ArgumentNullException.ThrowIfNull(bet);
        ValidateBet(bet);
        var won = BetWins(bet, winningPocket);
        var multiplier = bet.Kind == RouletteBetKind.Straight ? 36m : 2m;
        return new RouletteSettlement(
            bet.PlayerId,
            bet.Kind,
            bet.Stake,
            won,
            won ? bet.Stake * multiplier : 0m);
    }

    private static bool BetWins(RouletteBet bet, RoulettePocket pocket) => bet.Kind switch
    {
        RouletteBetKind.Straight => bet.Number == pocket.Number,
        RouletteBetKind.Red => !pocket.IsZero && pocket.IsRed,
        RouletteBetKind.Black => !pocket.IsZero && !pocket.IsRed,
        RouletteBetKind.Even => !pocket.IsZero && pocket.Number % 2 == 0,
        RouletteBetKind.Odd => !pocket.IsZero && pocket.Number % 2 != 0,
        RouletteBetKind.Low => pocket.Number is >= 1 and <= 18,
        RouletteBetKind.High => pocket.Number is >= 19 and <= 36,
        _ => throw new ArgumentOutOfRangeException(nameof(bet), "Unknown Roulette bet kind."),
    };

    private static void ValidateBet(RouletteBet bet)
    {
        if (string.IsNullOrWhiteSpace(bet.PlayerId))
            throw new ArgumentException("A player ID is required.", nameof(bet));
        if (bet.Stake <= 0m)
            throw new ArgumentOutOfRangeException(nameof(bet), "A Roulette stake must be positive.");
        if (!Enum.IsDefined(bet.Kind))
            throw new ArgumentOutOfRangeException(nameof(bet), "The Roulette bet kind is invalid.");
        if (bet.Kind == RouletteBetKind.Straight && bet.Number is not (>= 0 and <= 36))
            throw new ArgumentException("A straight bet requires a number from 0 through 36.", nameof(bet));
        if (bet.Kind != RouletteBetKind.Straight && bet.Number is not null)
            throw new ArgumentException("Only a straight bet may specify a number.", nameof(bet));
    }
}
