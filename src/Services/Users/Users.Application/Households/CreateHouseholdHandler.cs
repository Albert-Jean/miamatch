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
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public CreateHouseholdHandler(
            IHouseholdRepository householdRepository,
            IUserRepository userRepository,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _householdRepository = householdRepository;
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }
        public async Task<CreateHouseholdResult> ExecuteAsync(string name, Guid creatorUserId)
        {

            Household household = Household.Create(name, creatorUserId);
            await _householdRepository.AddAsync(household);

            // The creator's existing JWT was issued before this household existed, so it
            // carries no "householdId" claim for it — reissue a token with the updated set,
            // same as LoginHandler does, or every other service would reject calls for it.
            var user = await _userRepository.GetByIdAsync(creatorUserId);
            var householdIds = await _householdRepository.GetHouseholdIdsForUserAsync(creatorUserId);
            string token = _jwtTokenGenerator.GenerateToken(user!, householdIds);

            return new CreateHouseholdResult(household.Id, household.Name, household.InviteCode.Value, household.Members.Count, token);
        }
    }
    public sealed record CreateHouseholdResult(Guid HouseholdId, string Name, string InviteCode, int MemberCount, string Token);
}
