using Dyadic.Domain.Entities;
using FluentAssertions;

namespace Dyadic.UnitTests.Domain;

public class ApplicationUserTests {
    [Fact]
    public void CreatedAt_ShouldDefaultToUtcNow() {
        var before = DateTime.UtcNow;
        var user = new ApplicationUser { FullName = "Test User" };
        var after = DateTime.UtcNow;

        user.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void NavigationProperties_ShouldBeNullByDefault() {
        var user = new ApplicationUser { FullName = "Test User" };

        user.StudentProfile.Should().BeNull();
        user.SupervisorProfile.Should().BeNull();
    }
}