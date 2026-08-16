using System;
using System.Collections.Generic;
using System.Text;

namespace Matching.Domain.Entities
{
    public class Match
    {
        public Guid Id { get; }
        public Guid HouseholdId { get; }
        public Guid RecipeId { get; }
        public Guid DeckId { get; }
        public DateTime MatchedAt { get; }

        private Match(Guid id, Guid householdId, Guid recipeId, Guid deckId, DateTime matchedAt)
        {
            Id = id;
            HouseholdId = householdId;
            RecipeId = recipeId;
            DeckId = deckId;
            MatchedAt = matchedAt;
        }
        public static Match Create(Guid householdId, Guid recipeId, Guid deckId)
        {
            Guid id = Guid.NewGuid();
            DateTime matchedAt = DateTime.UtcNow;
            return new Match(id, householdId, recipeId, deckId, matchedAt);
        }
    }
}
