using Dyadic.Domain.Entities;
using Dyadic.Infrastructure.Services;
using Dyadic.UnitTests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Dyadic.UnitTests.Services;

public class UserAdminServiceTests
{
    // ── helpers ────────────────────────────────────────────────────────────────

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static ApplicationUser MakeUser(string name = "Test User", string role = "Student") =>
        new() { Id = Guid.NewGuid(), FullName = name, UserName = $"{name}@test.com", Email = $"{name}@test.com", IsActive = true };

    private static IQueryable<ApplicationUser> AsAsyncQueryable(IEnumerable<ApplicationUser> users) =>
        new TestAsyncEnumerable<ApplicationUser>(users);

    // ── positive tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllUsersAsync_ReturnsProjectedDTOs_WithRoles()
    {
        var user1 = MakeUser("Alice");
        var user2 = MakeUser("Bob");
        var um = CreateUserManagerMock();
        um.Setup(m => m.Users).Returns(AsAsyncQueryable(new[] { user1, user2 }));
        um.Setup(m => m.GetRolesAsync(user1)).ReturnsAsync(new List<string> { "Student" });
        um.Setup(m => m.GetRolesAsync(user2)).ReturnsAsync(new List<string> { "Supervisor" });

        var svc = new UserAdminService(um.Object);
        var result = await svc.GetAllUsersAsync();

        result.Should().HaveCount(2);
        result.First(r => r.FullName == "Alice").Role.Should().Be("Student");
        result.First(r => r.FullName == "Bob").Role.Should().Be("Supervisor");
    }

    [Fact]
    public async Task ChangeRoleAsync_RemovesExistingRoles_ThenAddsNewRole()
    {
        var user = MakeUser();
        var adminId = Guid.NewGuid();
        var um = CreateUserManagerMock();
        um.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        um.Setup(m => m.GetUsersInRoleAsync("Admin")).ReturnsAsync(new List<ApplicationUser>());
        um.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Student" });
        um.Setup(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
          .ReturnsAsync(IdentityResult.Success);
        um.Setup(m => m.AddToRoleAsync(user, "Supervisor")).ReturnsAsync(IdentityResult.Success);

        var svc = new UserAdminService(um.Object);
        await svc.ChangeRoleAsync(user.Id, "Supervisor", adminId);

        um.Verify(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()), Times.Once);
        um.Verify(m => m.AddToRoleAsync(user, "Supervisor"), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_SetsIsActiveFalse_AndCallsUpdateAsync()
    {
        var user = MakeUser();
        var adminId = Guid.NewGuid();
        var um = CreateUserManagerMock();
        um.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        um.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var svc = new UserAdminService(um.Object);
        await svc.DeactivateAsync(user.Id, adminId);

        user.IsActive.Should().BeFalse();
        um.Verify(m => m.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task ReactivateAsync_SetsIsActiveTrue_AndCallsUpdateAsync()
    {
        var user = MakeUser();
        user.IsActive = false;
        var um = CreateUserManagerMock();
        um.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        um.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var svc = new UserAdminService(um.Object);
        await svc.ReactivateAsync(user.Id);

        user.IsActive.Should().BeTrue();
        um.Verify(m => m.UpdateAsync(user), Times.Once);
    }

    // ── guard-rail tests ───────────────────────────────────────────────────────

    [Fact]
    public async Task ChangeRoleAsync_SelfRoleChange_Throws()
    {
        var adminId = Guid.NewGuid();
        var um = CreateUserManagerMock();
        var svc = new UserAdminService(um.Object);

        var act = () => svc.ChangeRoleAsync(adminId, "Student", adminId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot change your own role*");
    }

    [Fact]
    public async Task DeactivateAsync_SelfDeactivation_Throws()
    {
        var adminId = Guid.NewGuid();
        var um = CreateUserManagerMock();
        var svc = new UserAdminService(um.Object);

        var act = () => svc.DeactivateAsync(adminId, adminId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot deactivate your own account*");
    }

    [Fact]
    public async Task ChangeRoleAsync_DemotingLastAdmin_Throws()
    {
        var user = MakeUser();
        var adminId = Guid.NewGuid();
        var um = CreateUserManagerMock();
        um.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        um.Setup(m => m.GetUsersInRoleAsync("Admin"))
          .ReturnsAsync(new List<ApplicationUser> { user });

        var svc = new UserAdminService(um.Object);
        var act = () => svc.ChangeRoleAsync(user.Id, "Student", adminId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*last administrator*");
    }

    // ── rollback test ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ChangeRoleAsync_AddToRoleFails_RestoresOriginalRole_AndThrows()
    {
        var user = MakeUser();
        var adminId = Guid.NewGuid();
        var originalRole = "Student";
        var um = CreateUserManagerMock();
        um.Setup(m => m.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        um.Setup(m => m.GetUsersInRoleAsync("Admin")).ReturnsAsync(new List<ApplicationUser>());
        um.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { originalRole });
        um.Setup(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
          .ReturnsAsync(IdentityResult.Success);
        um.Setup(m => m.AddToRoleAsync(user, "Supervisor"))
          .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));
        um.Setup(m => m.AddToRoleAsync(user, originalRole)).ReturnsAsync(IdentityResult.Success);

        var svc = new UserAdminService(um.Object);
        var act = () => svc.ChangeRoleAsync(user.Id, "Supervisor", adminId);

        await act.Should().ThrowAsync<InvalidOperationException>();
        um.Verify(m => m.AddToRoleAsync(user, originalRole), Times.Once);
    }

    // ── not-found tests ────────────────────────────────────────────────────────

    [Fact]
    public async Task ChangeRoleAsync_UserNotFound_Throws()
    {
        var um = CreateUserManagerMock();
        um.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        var svc = new UserAdminService(um.Object);

        var act = () => svc.ChangeRoleAsync(Guid.NewGuid(), "Student", Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task DeactivateAsync_UserNotFound_Throws()
    {
        var um = CreateUserManagerMock();
        um.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        var svc = new UserAdminService(um.Object);

        var act = () => svc.DeactivateAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task ReactivateAsync_UserNotFound_Throws()
    {
        var um = CreateUserManagerMock();
        um.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
        var svc = new UserAdminService(um.Object);

        var act = () => svc.ReactivateAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }
}
