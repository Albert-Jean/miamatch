using FluentAssertions;
using Users.Domain.Entities;
using Users.Domain.Exceptions;

namespace Users.Domain.Tests.Entities;

public class HouseholdTests
{
    [Fact]
    public void Create_ReturnsHouseholdWithCreatorAsOnlyMember()
    {
        var creatorId = Guid.NewGuid();

        var household = Household.Create("Foyer Albert", creatorId);

        household.Id.Should().NotBeEmpty();
        household.Name.Should().Be("Foyer Albert");
        household.InviteCode.Value.Should().HaveLength(6);
        household.Members.Should().ContainSingle(m => m.UserId == creatorId);
    }

    [Fact]
    public void AddMember_WithNewUser_AddsMemberToHousehold()
    {
        var household = Household.Create("Foyer Albert", Guid.NewGuid());
        var newMemberId = Guid.NewGuid();

        household.AddMember(newMemberId);

        household.Members.Should().Contain(m => m.UserId == newMemberId);
        household.Members.Should().HaveCount(2);
    }

    [Fact]
    public void AddMember_WithUserAlreadyInHousehold_ThrowsAlreadyMemberException()
    {
        var creatorId = Guid.NewGuid();
        var household = Household.Create("Foyer Albert", creatorId);

        Action act = () => household.AddMember(creatorId);

        act.Should().Throw<AlreadyMemberException>();
    }

    [Fact]
    public void AddMember_WhenHouseholdHasTenMembers_ThrowsHouseholdFullException()
    {
        var household = Household.Create("Foyer Albert", Guid.NewGuid());
        for (int i = 0; i < 9; i++)
        {
            household.AddMember(Guid.NewGuid());
        }
        household.Members.Should().HaveCount(10);

        Action act = () => household.AddMember(Guid.NewGuid());

        act.Should().Throw<HouseholdFullException>();
    }

    [Fact]
    public void AddMember_WhenFullAndUserAlreadyMember_ThrowsHouseholdFullExceptionNotAlreadyMember()
    {
        var creatorId = Guid.NewGuid();
        var household = Household.Create("Foyer Albert", creatorId);
        for (int i = 0; i < 9; i++)
        {
            household.AddMember(Guid.NewGuid());
        }

        Action act = () => household.AddMember(creatorId);

        act.Should().Throw<HouseholdFullException>();
    }
}
