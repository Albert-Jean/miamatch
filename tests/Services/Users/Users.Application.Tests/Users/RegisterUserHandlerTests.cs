using FluentAssertions;
using NSubstitute;
using Users.Application.Abstractions;
using Users.Application.Users;
using Users.Domain.Entities;
using Users.Domain.Exceptions;
using Users.Domain.ValueObjects;

namespace Users.Application.Tests.Users;

public class RegisterUserHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        _handler = new RegisterUserHandler(_userRepository, _passwordHasher, _tokenGenerator);
    }

    [Fact]
    public async Task ExecuteAsync_WithNewEmail_RegistersUserAndReturnsToken()
    {
        _userRepository.GetByEmailAsync(Arg.Any<Email>()).Returns(Task.FromResult<User?>(null));
        _passwordHasher.Hash("plain-password").Returns("hashed-password");
        _tokenGenerator.GenerateToken(Arg.Any<User>(), Arg.Any<IEnumerable<Guid>>()).Returns("jwt-token");

        var result = await _handler.ExecuteAsync("Jean", "Jean@Example.com", "plain-password");

        result.Token.Should().Be("jwt-token");
        result.UserId.Should().NotBeEmpty();

        await _userRepository.Received(1).AddAsync(Arg.Is<User>(u =>
            u.Id == result.UserId &&
            u.Name == "Jean" &&
            u.Email.EmailAddress == "jean@example.com" &&
            u.PasswordHash == "hashed-password"));
        _tokenGenerator.Received(1).GenerateToken(Arg.Any<User>(), Arg.Is<IEnumerable<Guid>>(ids => !ids.Any()));
    }

    [Fact]
    public async Task ExecuteAsync_WithEmailAlreadyInUse_ThrowsEmailAlreadyInUseException()
    {
        var existingUser = User.Create("Existing", "jean@example.com", "hash");
        _userRepository.GetByEmailAsync(Arg.Any<Email>()).Returns(Task.FromResult<User?>(existingUser));

        Func<Task> act = () => _handler.ExecuteAsync("Jean", "jean@example.com", "plain-password");

        await act.Should().ThrowAsync<EmailAlreadyInUseException>();
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidEmail_ThrowsInvalidEmailExceptionAndDoesNotCreateUser()
    {
        Func<Task> act = () => _handler.ExecuteAsync("Jean", "not-an-email", "plain-password");

        await act.Should().ThrowAsync<InvalidEmailException>();
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>());
    }
}
