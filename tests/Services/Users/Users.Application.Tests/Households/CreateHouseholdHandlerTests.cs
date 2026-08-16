using FluentAssertions;
using NSubstitute;
using Users.Application.Abstractions;
using Users.Application.Households;
using Users.Domain.Entities;

namespace Users.Application.Tests.Households;

public class CreateHouseholdHandlerTests
{
    private readonly IHouseholdRepository _householdRepository = Substitute.For<IHouseholdRepository>();
    private readonly CreateHouseholdHandler _handler;

    public CreateHouseholdHandlerTests()
    {
        _handler = new CreateHouseholdHandler(_householdRepository);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidName_CreatesAndPersistsHousehold()
    {
        var creatorId = Guid.NewGuid();

        var result = await _handler.ExecuteAsync("Foyer Albert", creatorId);

        result.HouseholdID.Should().NotBeEmpty();
        result.InviteCode.Should().HaveLength(6);

        await _householdRepository.Received(1).AddAsync(Arg.Is<Household>(h =>
            h.Id == result.HouseholdID &&
            h.Name == "Foyer Albert" &&
            h.InviteCode.Value == result.InviteCode &&
            h.Members.Any(m => m.UserId == creatorId)));
    }
}
