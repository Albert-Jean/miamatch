using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.Abstractions;
using Users.Domain.Entities;

namespace Users.Application.Households
{
    public class CreateHouseholdHandler
    {

        private readonly IHouseholdRepository _householdRepository;

        public CreateHouseholdHandler(

            IHouseholdRepository householdRepository)
        {
            _householdRepository = householdRepository;
        }
        public async Task<CreatehouseholdResult> ExecuteAsync(string name, Guid creatorUserId)
        {
            
            Household household = Household.Create(name, creatorUserId);
            Guid householdId = household.Id;
            string inviteCode = household.InviteCode.Value;
            await _householdRepository.AddAsync(household);

            return new CreatehouseholdResult(householdId,inviteCode);
        }
    }    
    public sealed record CreatehouseholdResult(Guid HouseholdID, string InviteCode);
}
