using FluentAssertions;
using NSubstitute;
using Users.Application.Abstractions;
using Users.Application.Users;
using Users.Domain.Entities;
using Users.Domain.Exceptions;
using Users.Domain.ValueObjects;

namespace Users.Application.Tests.Users;

public class LoginHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly IJwtTokenGenerator _tokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly IHouseholdRepository _householdRepository = Substitute.For<IHouseholdRepository>();
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _handler = new LoginHandler(_userRepository, _passwordHasher, _tokenGenerator, _householdRepository);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCredentials_ReturnsUserIdAndToken()
    {
        var user = User.Create("Jean", "jean@example.com", "hashed-password");
        var householdIds = new[] { Guid.NewGuid() };
        _userRepository.GetByEmailAsync(Arg.Any<Email>()).Returns(Task.FromResult<User?>(user));
        _passwordHasher.Verify("plain-password", "hashed-password").Returns(true);
        _householdRepository.GetHouseholdIdsForUserAsync(user.Id).Returns(Task.FromResult<IReadOnlyCollection<Guid>>(householdIds));
        _tokenGenerator.GenerateToken(user, householdIds).Returns("jwt-token");

        var result = await _handler.ExecuteAsync("jean@example.com", "plain-password");

        result.UserId.Should().Be(user.Id);
        result.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownEmail_ThrowsInvalidCredentialsException()
    {
        _userRepository.GetByEmailAsync(Arg.Any<Email>()).Returns(Task.FromResult<User?>(null));

        Func<Task> act = () => _handler.ExecuteAsync("jean@example.com", "plain-password");

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task ExecuteAsync_WithWrongPassword_ThrowsInvalidCredentialsException()
    {
        var user = User.Create("Jean", "jean@example.com", "hashed-password");
        _userRepository.GetByEmailAsync(Arg.Any<Email>()).Returns(Task.FromResult<User?>(user));
        _passwordHasher.Verify("wrong-password", "hashed-password").Returns(false);

        Func<Task> act = () => _handler.ExecuteAsync("jean@example.com", "wrong-password");

        await act.Should().ThrowAsync<InvalidCredentialsException>();
        _tokenGenerator.DidNotReceive().GenerateToken(Arg.Any<User>(), Arg.Any<IEnumerable<Guid>>());
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidEmailFormat_ThrowsInvalidEmailException()
    {
        Func<Task> act = () => _handler.ExecuteAsync("not-an-email", "plain-password");

        await act.Should().ThrowAsync<InvalidEmailException>();
    }
}
