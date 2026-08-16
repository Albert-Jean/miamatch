using System;
using System.Collections.Generic;
using System.Text;

namespace Matching.Domain.Entities
{
    public class Swipe
    {
        public Guid Id { get; }
        public Guid UserId { get; }
        public Guid HouseholdId { get; }
        public Guid RecipeId { get; }
        public Guid DeckId { get; }
        public bool Liked { get; private set; }
        public DateTime SwipedAt { get; private set; }

        private Swipe(Guid id,Guid userId, Guid householdId, Guid recipeId, Guid deckId,bool liked,DateTime swipedAt)
        {
            Id=id;
            UserId = userId;
            HouseholdId = householdId;
            RecipeId = recipeId;
            DeckId = deckId;
            Liked = liked;
            SwipedAt = swipedAt;
        }
        public static Swipe Create(Guid userId, Guid householdId, Guid recipeId, Guid deckId, bool liked)
        {
            Guid id = Guid.NewGuid();
            DateTime swipedAt = DateTime.UtcNow;
            return new Swipe(id, userId, householdId, recipeId, deckId, liked, swipedAt);
        }

        public void ChangeDecision(bool liked)
        {
            SwipedAt = DateTime.UtcNow;
            Liked = liked;
        }
    }
}
