using FluentAssertions;
using Users.Domain.Exceptions;
using Users.Domain.ValueObjects;

namespace Users.Domain.Tests.ValueObjects;

public class InviteCodeTests
{
    private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    [Fact]
    public void Generate_ReturnsSixCharacterCodeUsingOnlyAllowedChars()
    {
        var inviteCode = InviteCode.Generate();

        inviteCode.Value.Should().HaveLength(6);
        inviteCode.Value.ToCharArray().Should().OnlyContain(c => AllowedChars.Contains(c));
    }

    [Fact]
    public void From_WithLowerCaseValidCode_ReturnsUpperCasedInviteCode()
    {
        var inviteCode = InviteCode.From("ab2c3d");

        inviteCode.Value.Should().Be("AB2C3D");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void From_WithNullOrEmptyCode_ThrowsInvalidInviteCodeException(string? code)
    {
        Action act = () => InviteCode.From(code!);

        act.Should().Throw<InvalidInviteCodeException>();
    }

    [Theory]
    [InlineData("ABC12")]
    [InlineData("ABCDEFG")]
    public void From_WithWrongLength_ThrowsInvalidInviteCodeException(string code)
    {
        Action act = () => InviteCode.From(code);

        act.Should().Throw<InvalidInviteCodeException>()
            .WithMessage($"*{code}*");
    }

    [Theory]
    [InlineData("ABCD0E")]
    [InlineData("ABCD1E")]
    [InlineData("ABCDIE")]
    [InlineData("ABCDOE")]
    public void From_WithDisallowedCharacters_ThrowsInvalidInviteCodeException(string code)
    {
        Action act = () => InviteCode.From(code);

        act.Should().Throw<InvalidInviteCodeException>();
    }
}
