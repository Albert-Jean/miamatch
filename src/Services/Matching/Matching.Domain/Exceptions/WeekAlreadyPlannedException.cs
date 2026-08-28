using System;

namespace Matching.Domain.Exceptions
{
    /// <summary>
    /// Raised when a household tries to swipe on a deck that already reached the number of
    /// meals it was created for. The week is planned, so further swipes are refused.
    /// </summary>
    public class WeekAlreadyPlannedException : Exception
    {
        public WeekAlreadyPlannedException(Guid deckId, int mealCount)
            : base($"Deck {deckId} already reached its {mealCount} planned meals.")
        {
            DeckId = deckId;
            MealCount = mealCount;
        }

        public Guid DeckId { get; }
        public int MealCount { get; }
    }
}
