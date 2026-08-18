using FortuneForge.Games.Cards;
using FortuneForge.Games.TrickTaking;

namespace FortuneForge.Games.Spades;

public static class SpadesEngine
{
    public static SpadesState Start(uint seed, PlayerSeat dealer = PlayerSeat.West)
    {
        var firstPlayer = dealer.Next();
        var deal = TrickTakingDealer.Deal(seed, firstPlayer);
        return new SpadesState(
            seed,
            dealer,
            firstPlayer,
            SpadesPhase.Bidding,
            deal.Hands,
            [],
            new TrickState(firstPlayer, []),
            [],
            Enum.GetValues<PlayerSeat>().Select(seat => new SpadesTrickCount(seat, 0)).ToArray(),
            false,
            null);
    }

    public static SpadesTransition Apply(SpadesState state, SpadesCommand command)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(command);
        return command switch
        {
            PlaceSpadesBid bid => Bid(state, bid),
            PlaySpadesCard play => Play(state, play),
            _ => throw new SpadesRuleException("The Spades command is not supported."),
        };
    }

    private static SpadesTransition Bid(SpadesState state, PlaceSpadesBid command)
    {
        RequirePhase(state, SpadesPhase.Bidding);
        RequireTurn(state, command.Seat);
        if (command.Tricks is < 1 or > TrickTakingRules.CardsPerPlayer)
            throw new SpadesRuleException("A basic Spades bid must be from one through thirteen tricks.");
        if (state.Bids.Any(bid => bid.Seat == command.Seat))
            throw new SpadesRuleException("That player has already bid.");

        var bids = state.Bids.Append(new SpadesBid(command.Seat, command.Tricks)).ToArray();
        var biddingComplete = bids.Length == TrickTakingRules.PlayerCount;
        var next = state with
        {
            Bids = bids,
            Phase = biddingComplete ? SpadesPhase.Playing : SpadesPhase.Bidding,
            Turn = biddingComplete ? state.CurrentTrick.Leader : command.Seat.Next(),
        };
        return new SpadesTransition(next, SpadesEventType.BidPlaced, $"{command.Seat} bid {command.Tricks}.");
    }

    private static SpadesTransition Play(SpadesState state, PlaySpadesCard command)
    {
        RequirePhase(state, SpadesPhase.Playing);
        RequireTurn(state, command.Seat);
        var hand = state.HandFor(command.Seat);
        if (state.CurrentTrick.Plays.Count == 0 &&
            command.Card.Suit == CardSuit.Spades &&
            !state.SpadesBroken &&
            hand.Any(card => card.Suit != CardSuit.Spades))
        {
            throw new SpadesRuleException("Spades cannot lead until broken while another suit is available.");
        }

        ValidateSharedPlay(hand, state.CurrentTrick, command);
        var trick = state.CurrentTrick with
        {
            Plays = state.CurrentTrick.Plays.Append(new TrickPlay(command.Seat, command.Card)).ToArray(),
        };
        var hands = ReplaceHand(state.Hands, command.Seat, hand.Where(card => card != command.Card).ToArray());
        var played = state with
        {
            Hands = hands,
            CurrentTrick = trick,
            Turn = command.Seat.Next(),
            SpadesBroken = state.SpadesBroken || command.Card.Suit == CardSuit.Spades,
        };

        if (!trick.IsComplete)
            return new SpadesTransition(played, SpadesEventType.CardPlayed, $"{command.Seat} played {command.Card.Code}.");

        return CompleteTrick(played, trick);
    }

    private static SpadesTransition CompleteTrick(SpadesState state, TrickState trick)
    {
        var winner = TrickTakingRules.WinningSeat(trick, CardSuit.Spades);
        var completed = new CompletedTrick(
            state.CompletedTricks.Count + 1,
            trick.Leader,
            winner,
            trick.Plays);
        var tricks = state.TricksWon
            .Select(count => count.Seat == winner ? count with { Tricks = count.Tricks + 1 } : count)
            .ToArray();
        var history = state.CompletedTricks.Append(completed).ToArray();
        var roundComplete = history.Length == TrickTakingRules.CardsPerPlayer;
        var next = state with
        {
            Turn = winner,
            CurrentTrick = new TrickState(winner, []),
            CompletedTricks = history,
            TricksWon = tricks,
            Phase = roundComplete ? SpadesPhase.Complete : SpadesPhase.Playing,
            Score = roundComplete ? ScoreRound(state.Bids, tricks) : null,
        };
        return new SpadesTransition(
            next,
            roundComplete ? SpadesEventType.RoundCompleted : SpadesEventType.TrickCompleted,
            roundComplete ? $"{winner} took the final trick." : $"{winner} took the trick.");
    }

    private static SpadesTeamScore ScoreRound(
        IReadOnlyList<SpadesBid> bids,
        IReadOnlyList<SpadesTrickCount> tricks) => new(
        ScoreTeam([PlayerSeat.North, PlayerSeat.South], bids, tricks),
        ScoreTeam([PlayerSeat.East, PlayerSeat.West], bids, tricks));

    private static int ScoreTeam(
        IReadOnlyList<PlayerSeat> seats,
        IReadOnlyList<SpadesBid> bids,
        IReadOnlyList<SpadesTrickCount> tricks)
    {
        var contract = bids.Where(bid => seats.Contains(bid.Seat)).Sum(bid => bid.Tricks);
        var won = tricks.Where(count => seats.Contains(count.Seat)).Sum(count => count.Tricks);
        return won >= contract ? (contract * 10) + won - contract : contract * -10;
    }

    private static IReadOnlyList<PlayerHand> ReplaceHand(
        IReadOnlyList<PlayerHand> hands,
        PlayerSeat seat,
        IReadOnlyList<PlayingCard> cards) => hands
        .Select(hand => hand.Seat == seat ? hand with { Cards = cards } : hand)
        .ToArray();

    private static void ValidateSharedPlay(
        IReadOnlyList<PlayingCard> hand,
        TrickState trick,
        PlaySpadesCard command)
    {
        try
        {
            TrickTakingRules.ValidatePlay(hand, trick, command.Seat, command.Card);
        }
        catch (TrickTakingRuleException exception)
        {
            throw new SpadesRuleException(exception.Message);
        }
    }

    private static void RequirePhase(SpadesState state, SpadesPhase phase)
    {
        if (state.Phase != phase)
            throw new SpadesRuleException($"Spades is in the {state.Phase} phase.");
    }

    private static void RequireTurn(SpadesState state, PlayerSeat seat)
    {
        if (state.Turn != seat)
            throw new SpadesRuleException($"It is {state.Turn}'s turn.");
    }
}
