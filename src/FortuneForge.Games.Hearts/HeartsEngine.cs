using FortuneForge.Games.Cards;
using FortuneForge.Games.TrickTaking;

namespace FortuneForge.Games.Hearts;

public static class HeartsEngine
{
    private static readonly PlayingCard OpeningCard = new(CardRank.Two, CardSuit.Clubs);

    public static HeartsState Start(uint seed)
    {
        var deal = TrickTakingDealer.Deal(seed);
        var firstPlayer = deal.Hands.Single(hand => hand.Cards.Contains(OpeningCard)).Seat;
        return new HeartsState(
            seed,
            firstPlayer,
            HeartsPhase.Playing,
            deal.Hands,
            new TrickState(firstPlayer, []),
            [],
            Enum.GetValues<PlayerSeat>().Select(seat => new HeartsScore(seat, 0)).ToArray(),
            false);
    }

    public static HeartsTransition Apply(HeartsState state, HeartsCommand command)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(command);
        if (state.Phase != HeartsPhase.Playing)
            throw new HeartsRuleException("The Hearts round is complete.");
        if (state.Turn != command.Seat)
            throw new HeartsRuleException($"It is {state.Turn}'s turn.");

        return command switch
        {
            PlayHeartsCard play => Play(state, play),
            _ => throw new HeartsRuleException("The Hearts command is not supported."),
        };
    }

    private static HeartsTransition Play(HeartsState state, PlayHeartsCard command)
    {
        var hand = state.HandFor(command.Seat);
        ValidateGameRules(state, command, hand);
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
            HeartsBroken = state.HeartsBroken || command.Card.Suit == CardSuit.Hearts,
        };

        if (!trick.IsComplete)
            return new HeartsTransition(played, HeartsEventType.CardPlayed, $"{command.Seat} played {command.Card.Code}.");

        return CompleteTrick(played, trick);
    }

    private static HeartsTransition CompleteTrick(HeartsState state, TrickState trick)
    {
        var winner = TrickTakingRules.WinningSeat(trick);
        var trickPoints = trick.Plays.Sum(play => PointValue(play.Card));
        var scores = state.Scores
            .Select(score => score.Seat == winner ? score with { Points = score.Points + trickPoints } : score)
            .ToArray();
        var completed = new CompletedTrick(
            state.CompletedTricks.Count + 1,
            trick.Leader,
            winner,
            trick.Plays);
        var history = state.CompletedTricks.Append(completed).ToArray();
        var roundComplete = history.Length == TrickTakingRules.CardsPerPlayer;
        if (roundComplete)
            scores = ApplyShootingTheMoon(scores);

        var next = state with
        {
            Turn = winner,
            CurrentTrick = new TrickState(winner, []),
            CompletedTricks = history,
            Scores = scores,
            Phase = roundComplete ? HeartsPhase.Complete : HeartsPhase.Playing,
        };
        return new HeartsTransition(
            next,
            roundComplete ? HeartsEventType.RoundCompleted : HeartsEventType.TrickCompleted,
            roundComplete ? $"{winner} took the final trick." : $"{winner} took {trickPoints} points.");
    }

    private static void ValidateGameRules(
        HeartsState state,
        PlayHeartsCard command,
        IReadOnlyList<PlayingCard> hand)
    {
        var openingPlay = state.CompletedTricks.Count == 0 && state.CurrentTrick.Plays.Count == 0;
        if (openingPlay && command.Card != OpeningCard)
            throw new HeartsRuleException("The first trick must be led with the two of clubs.");

        if (state.CurrentTrick.Plays.Count == 0 &&
            command.Card.Suit == CardSuit.Hearts &&
            !state.HeartsBroken &&
            hand.Any(card => card.Suit != CardSuit.Hearts))
        {
            throw new HeartsRuleException("Hearts cannot lead until broken while another suit is available.");
        }

        var firstTrick = state.CompletedTricks.Count == 0;
        if (firstTrick && PointValue(command.Card) > 0)
        {
            var legalCards = TrickTakingRules.LegalCards(hand, state.CurrentTrick);
            if (legalCards.Any(card => PointValue(card) == 0))
                throw new HeartsRuleException("A point card cannot be played on the first trick while a safe card is available.");
        }
    }

    private static int PointValue(PlayingCard card) => card switch
    {
        { Suit: CardSuit.Hearts } => 1,
        { Suit: CardSuit.Spades, Rank: CardRank.Queen } => 13,
        _ => 0,
    };

    private static HeartsScore[] ApplyShootingTheMoon(IReadOnlyList<HeartsScore> scores)
    {
        var shooter = scores.SingleOrDefault(score => score.Points == 26);
        return shooter is null
            ? scores.ToArray()
            : scores.Select(score => score with { Points = score.Seat == shooter.Seat ? 0 : 26 }).ToArray();
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
        PlayHeartsCard command)
    {
        try
        {
            TrickTakingRules.ValidatePlay(hand, trick, command.Seat, command.Card);
        }
        catch (TrickTakingRuleException exception)
        {
            throw new HeartsRuleException(exception.Message);
        }
    }
}
