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
    public class RegisterUserHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _tokenGenerator;

        public RegisterUserHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator tokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenGenerator = tokenGenerator;
        }
        public async Task<RegisterUserResult> ExecuteAsync(string name, string email, string password)
        {
            // Build Email and check if it's not already in use 
            Email validEmail = Email.Create(email);
            var existing = await _userRepository.GetByEmailAsync(validEmail);
            if (existing is not null) throw new EmailAlreadyInUseException(email);
            // hash password
            var passwordHash = _passwordHasher.Hash(password);
            var user = User.Create(name, email, passwordHash);
            await _userRepository.AddAsync(user);
            //Generate jwt token
            var token = _tokenGenerator.GenerateToken(user, Array.Empty<Guid>());

            return new RegisterUserResult(user.Id, token);
        }
    }
    public sealed record RegisterUserResult(Guid UserId, string Token);
}
