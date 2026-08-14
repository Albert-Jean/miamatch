using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Users.Application.Abstractions;
using Users.Domain.Entities;
using Users.Domain.Exceptions;
using Users.Domain.ValueObjects;

namespace Users.Application.Households
{
    public class JoinHouseholdHandler
    {
        private readonly IHouseholdRepository _householdRepository;

        public JoinHouseholdHandler(

            IHouseholdRepository householdRepository)
        {
            _householdRepository = householdRepository;
        }
        public async Task<JoinHouseholdResult> ExecuteAsync(string rawInviteCode, Guid userId)
        {
            InviteCode inviteCode = InviteCode.From(rawInviteCode);
            var household = await _householdRepository.GetByInviteCodeAsync(inviteCode);
            if(household == null)
            {
                throw new HouseHoldNotFoundException();
            }
            household.AddMember(userId);
            await _householdRepository.UpdateAsync(household);
            return new JoinHouseholdResult(household.Id, household.Name, household.InviteCode.Value, household.Members.Count);
        }
    }
    public sealed record JoinHouseholdResult(Guid HouseholdId, string Name, string InviteCode, int MemberCount);
}
