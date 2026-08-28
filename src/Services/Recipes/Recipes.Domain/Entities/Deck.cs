using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Recipe.Domain.Entities
{
    public class Deck
    {
        public Guid Id { get; }
        public Guid HouseholdId {  get; }
        public DateTime GeneratedAt { get; }
        /// <summary>
        /// How many meals the household wants this week; swiping stops once that many matches exist.
        /// Zero means no limit, which is also what decks created before this rule existed carry.
        /// </summary>
        public int MealCount { get; }
        private List<Guid> _recipeIds;
        public IReadOnlyCollection<Guid> RecipeIds => _recipeIds.AsReadOnly();

        private Deck(Guid id, Guid householdId, DateTime generatedAt, int mealCount, List<Guid> recipeIds)
        {
            Id = id;
            HouseholdId = householdId;
            GeneratedAt = generatedAt;
            MealCount = mealCount;
            _recipeIds = recipeIds;
        }
        private Deck(Guid id, Guid householdId, DateTime generatedAt, int mealCount)
        {
            Id = id;
            HouseholdId = householdId;
            GeneratedAt = generatedAt;
            MealCount = mealCount;
            _recipeIds = new List<Guid>();
        }
        public static Deck Create(Guid householdId, IEnumerable<Guid> recipeIds, int mealCount)
        {
            var ids = recipeIds.ToList();
            if (mealCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(mealCount), "A deck cannot plan a negative number of meals.");
            }
            Guid id = Guid.NewGuid();
            DateTime generatedAt = DateTime.UtcNow;
            return new Deck(id, householdId, generatedAt, mealCount, ids);
        }
        public bool IsExpired(DateTime now)
        {
            var result = now - GeneratedAt;
            return result > TimeSpan.FromDays(7);
        }
    }
}
