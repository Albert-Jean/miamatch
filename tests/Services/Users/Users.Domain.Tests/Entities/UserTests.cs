using FluentAssertions;
using Users.Domain.Entities;
using Users.Domain.Exceptions;

namespace Users.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ReturnsUserWithExpectedValues()
    {
        var before = DateTime.UtcNow;

        var user = User.Create("Jean", "Jean@Example.com", "hashed-password");

        var after = DateTime.UtcNow;

        user.Id.Should().NotBeEmpty();
        user.Name.Should().Be("Jean");
        user.Email.EmailAddress.Should().Be("jean@example.com");
        user.PasswordHash.Should().Be("hashed-password");
        user.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Create_CalledTwice_GeneratesDistinctIds()
    {
        var first = User.Create("Jean", "jean@example.com", "hash");
        var second = User.Create("Jean", "jean@example.com", "hash");

        first.Id.Should().NotBe(second.Id);
    }

    [Fact]
    public void Create_WithInvalidEmail_ThrowsInvalidEmailException()
    {
        Action act = () => User.Create("Jean", "not-an-email", "hash");

        act.Should().Throw<InvalidEmailException>();
    }
}
