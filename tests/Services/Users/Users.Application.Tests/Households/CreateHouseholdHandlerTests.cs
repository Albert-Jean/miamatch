using FluentAssertions;
using NSubstitute;
using Users.Application.Abstractions;
using Users.Application.Households;
using Users.Domain.Entities;

namespace Users.Application.Tests.Households;

public class CreateHouseholdHandlerTests
{
    private readonly IHouseholdRepository _householdRepository = Substitute.For<IHouseholdRepository>();
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IJwtTokenGenerator _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
    private readonly CreateHouseholdHandler _handler;

    public CreateHouseholdHandlerTests()
    {
        _handler = new CreateHouseholdHandler(_householdRepository, _userRepository, _jwtTokenGenerator);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidName_CreatesAndPersistsHousehold()
    {
        var creatorId = Guid.NewGuid();
        var creator = User.Create("Albert", "albert@example.com", "hash");
        _userRepository.GetByIdAsync(creatorId).Returns(Task.FromResult<User?>(creator));
        _householdRepository.GetHouseholdIdsForUserAsync(creatorId).Returns(Task.FromResult<IReadOnlyCollection<Guid>>([]));
        _jwtTokenGenerator.GenerateToken(creator, Arg.Any<IEnumerable<Guid>>()).Returns("fresh-token");

        var result = await _handler.ExecuteAsync("Foyer Albert", creatorId);

        result.HouseholdId.Should().NotBeEmpty();
        result.Name.Should().Be("Foyer Albert");
        result.InviteCode.Should().HaveLength(6);
        result.MemberCount.Should().Be(1);
        result.Token.Should().Be("fresh-token");

        await _householdRepository.Received(1).AddAsync(Arg.Is<Household>(h =>
            h.Id == result.HouseholdId &&
            h.Name == "Foyer Albert" &&
            h.InviteCode.Value == result.InviteCode &&
            h.Members.Any(m => m.UserId == creatorId)));
    }
}
