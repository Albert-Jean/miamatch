using FluentAssertions;
using NSubstitute;
using Users.Application.Abstractions;
using Users.Application.Households;
using Users.Domain.Entities;
using Users.Domain.Exceptions;
using Users.Domain.ValueObjects;

namespace Users.Application.Tests.Households;

public class JoinHouseholdHandlerTests
{
    private readonly IHouseholdRepository _householdRepository = Substitute.For<IHouseholdRepository>();
    private readonly JoinHouseholdHandler _handler;

    public JoinHouseholdHandlerTests()
    {
        _handler = new JoinHouseholdHandler(_householdRepository);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInviteCode_AddsMemberAndUpdatesHousehold()
    {
        var household = Household.Create("Foyer Albert", Guid.NewGuid());
        var newUserId = Guid.NewGuid();
        _householdRepository.GetByInviteCodeAsync(Arg.Is<InviteCode>(c => c.Value == household.InviteCode.Value))
            .Returns(Task.FromResult<Household?>(household));

        var result = await _handler.ExecuteAsync(household.InviteCode.Value.ToLowerInvariant(), newUserId);

        result.HouseholdId.Should().Be(household.Id);
        result.MemberCount.Should().Be(2);
        household.Members.Should().Contain(m => m.UserId == newUserId);
        await _householdRepository.Received(1).UpdateAsync(household);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownInviteCode_ThrowsHouseHoldNotFoundException()
    {
        _householdRepository.GetByInviteCodeAsync(Arg.Any<InviteCode>()).Returns(Task.FromResult<Household?>(null));

        Func<Task> act = () => _handler.ExecuteAsync("ABC234", Guid.NewGuid());

        await act.Should().ThrowAsync<HouseHoldNotFoundException>();
        await _householdRepository.DidNotReceive().UpdateAsync(Arg.Any<Household>());
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidInviteCodeFormat_ThrowsInvalidInviteCodeException()
    {
        Func<Task> act = () => _handler.ExecuteAsync("bad-code", Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidInviteCodeException>();
        await _householdRepository.DidNotReceive().GetByInviteCodeAsync(Arg.Any<InviteCode>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserAlreadyMember_ThrowsAlreadyMemberException()
    {
        var creatorId = Guid.NewGuid();
        var household = Household.Create("Foyer Albert", creatorId);
        _householdRepository.GetByInviteCodeAsync(Arg.Any<InviteCode>()).Returns(Task.FromResult<Household?>(household));

        Func<Task> act = () => _handler.ExecuteAsync(household.InviteCode.Value, creatorId);

        await act.Should().ThrowAsync<AlreadyMemberException>();
        await _householdRepository.DidNotReceive().UpdateAsync(Arg.Any<Household>());
    }
}
