using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Users.Application.Abstractions;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Infrastructure.Persistence.Repositories
{
    public class HouseholdRepository : IHouseholdRepository
    {
        private readonly UsersDbContext _context;

        public HouseholdRepository(UsersDbContext context)
        {
            _context = context;
        }

        public async Task<Household?> GetByIdAsync(Guid id)
        {
            return await _context.Households.FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<Household?> GetByInviteCodeAsync(InviteCode code)
        {
            return await _context.Households.FirstOrDefaultAsync(h => h.InviteCode == code);
        }

        public async Task<IReadOnlyCollection<Guid>> GetHouseholdIdsForUserAsync(Guid userId)
        {
            return await _context.Households
                .Where(h => h.Members.Any(m => m.UserId == userId))
                .Select(h => h.Id)
                .ToListAsync();
        }

        public async Task AddAsync(Household household)
        {
            await _context.Households.AddAsync(household);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Household household)
        {
            await _context.SaveChangesAsync();
        }
    }
}
