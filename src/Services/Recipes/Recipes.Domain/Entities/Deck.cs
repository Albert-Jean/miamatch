using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Recipe.Domain.Entities
{
    public class Deck
    {
        public Guid Id { get; }
        public Guid HouseholdId {  get; }
        public DateTime GeneratedAt { get; }
        private List<Guid> _recipeIds;
        public IReadOnlyCollection<Guid> RecipeIds => _recipeIds.AsReadOnly();

        private Deck(Guid id, Guid householdId, DateTime generatedAt, List<Guid> recipeIds)
        {
            Id = id;
            HouseholdId = householdId;
            GeneratedAt = generatedAt;
            _recipeIds = recipeIds;
        }
        private Deck(Guid id, Guid householdId, DateTime generatedAt)
        {
            Id = id;
            HouseholdId = householdId;
            GeneratedAt = generatedAt;
            _recipeIds = new List<Guid>();
        }
        public static Deck Create(Guid householdId, IEnumerable<Guid> recipeIds)
        {
            Guid id = Guid.NewGuid();
            DateTime generatedAt = DateTime.UtcNow;
            return new Deck(id, householdId, generatedAt, recipeIds.ToList());
        }
        public bool IsExpired(DateTime now)
        {
            var result = now - GeneratedAt;
            return result > TimeSpan.FromDays(7);
        }
    }
}
