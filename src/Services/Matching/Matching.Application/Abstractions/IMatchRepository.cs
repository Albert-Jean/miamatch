using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matching.Domain.Entities;

namespace Matching.Application.Abstractions
{
    public interface IMatchRepository
    {
        Task<Match?> GetAsync(Guid householdId, Guid recipeId, Guid deckId);
        Task<IReadOnlyCollection<Match>> GetForHouseholdAsync(Guid householdId);
        Task<int> CountForDeckAsync(Guid householdId, Guid deckId);
        Task AddAsync(Match match);
    }
}
