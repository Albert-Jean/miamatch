using FluentAssertions;
using Users.Domain.Exceptions;
using Users.Domain.ValueObjects;

namespace Users.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_NormalizesToLowerCase()
    {
        var email = Email.Create("Test@Example.COM");

        email.EmailAddress.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_WithNullEmail_ThrowsArgumentNullException()
    {
        Action act = () => Email.Create(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    [InlineData("@missing-local.com")]
    public void Create_WithInvalidEmail_ThrowsInvalidEmailException(string invalidEmail)
    {
        Action act = () => Email.Create(invalidEmail);

        act.Should().Throw<InvalidEmailException>()
            .WithMessage($"*{invalidEmail}*");
    }

    [Fact]
    public void Create_WithSameAddressDifferentCase_ProducesEqualEmails()
    {
        var first = Email.Create("User@Example.com");
        var second = Email.Create("user@example.com");

        first.Should().Be(second);
    }
}
