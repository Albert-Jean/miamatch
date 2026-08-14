using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.Abstractions;
using Users.Domain.Entities;
using Users.Domain.Exceptions;
using Users.Domain.ValueObjects;

namespace Users.Application.Users
{
    public class LoginHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IHouseholdRepository _householdRepository;

        public LoginHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator,
            IHouseholdRepository householdRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
            _householdRepository = householdRepository;
        }

        public async Task<LoginResult> ExecuteAsync(string email, string password)
        {
            Email validEmail = Email.Create(email);
            var user = await _userRepository.GetByEmailAsync(validEmail);
            if (user is null) throw new InvalidCredentialsException();
            if(!_passwordHasher.Verify(password, user.PasswordHash))
            {
                throw new InvalidCredentialsException();
            }
            string token = _jwtTokenGenerator.GenerateToken(user, await _householdRepository.GetHouseholdIdsForUserAsync(user.Id));

            return new LoginResult(user.Id,token);
        }

        public sealed record LoginResult(Guid UserId, string Token);
    }
}
