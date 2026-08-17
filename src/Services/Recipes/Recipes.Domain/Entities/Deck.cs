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
        private List<Guid> _recipesId;
        public IReadOnlyCollection<Guid> RecipesId => _recipesId.AsReadOnly();

        private Deck(Guid id, Guid householdId, DateTime generatedAt, List<Guid> recipesId)
        {
            Id = id;
            HouseholdId = householdId;
            GeneratedAt = generatedAt;
            _recipesId = recipesId;
        }
        public static Deck Create(Guid householdId, IEnumerable<Guid> recipesId)
        {
            Guid id = Guid.NewGuid();
            DateTime generatedAt = DateTime.UtcNow;
            return new Deck(id, householdId, generatedAt, recipesId.ToList());
        }
        public bool IsExpired(DateTime now)
        {
            var result = now - GeneratedAt;
            return result > TimeSpan.FromDays(7);
        }
    }
}
