using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Application.Abstractions
{
    public interface IHouseholdRepository
    {
        Task<Household?> GetByIdAsync(Guid id);
        Task<Household?> GetByInviteCodeAsync(InviteCode code);
        Task<IReadOnlyCollection<Guid>> GetHouseholdIdsForUserAsync(Guid userId);
        Task AddAsync(Household household);
        Task UpdateAsync(Household household);
    }
}
