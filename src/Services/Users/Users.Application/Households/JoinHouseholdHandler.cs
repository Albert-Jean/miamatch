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
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public JoinHouseholdHandler(
            IHouseholdRepository householdRepository,
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _householdRepository = householdRepository;
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
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

            // Same as CreateHouseholdHandler: the caller's JWT predates this membership,
            // so it must be reissued with the updated household claims.
            var user = await _userRepository.GetByIdAsync(userId);
            var householdIds = await _householdRepository.GetHouseholdIdsForUserAsync(userId);
            string token = _jwtTokenGenerator.GenerateToken(user!, householdIds);

            return new JoinHouseholdResult(household.Id, household.Name, household.InviteCode.Value, household.Members.Count, token);
        }
    }
    public sealed record JoinHouseholdResult(Guid HouseholdId, string Name, string InviteCode, int MemberCount, string Token);
}
