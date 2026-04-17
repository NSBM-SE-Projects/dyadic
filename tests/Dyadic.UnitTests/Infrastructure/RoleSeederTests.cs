using Dyadic.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using FluentAssertions;

namespace Dyadic.UnitTests.Infrastructure;

public class RoleSeederTests {
    private readonly Mock<RoleManager<IdentityRole<Guid>>> _roleManagerMock;
    private readonly ServiceProvider _serviceProvider;

    public RoleSeederTests() {
        _roleManagerMock = new Mock<RoleManager<IdentityRole<Guid>>>(
            Mock.Of<IRoleStore<IdentityRole<Guid>>>(),
            null!, null!, null!, null!);

        var services = new ServiceCollection();
        services.AddSingleton(_roleManagerMock.Object);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task SeedRolesAsync_WhenRolesDoNotExist_CreatesAllThreeRoles() {
        _roleManagerMock.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        _roleManagerMock.Setup(r => r.CreateAsync(It.IsAny<IdentityRole<Guid>>()))
            .ReturnsAsync(IdentityResult.Success);

        await RoleSeeder.SeedRolesAsync(_serviceProvider);

        _roleManagerMock.Verify(r => r.CreateAsync(It.Is<IdentityRole<Guid>>(role => role.Name == "Student")), Times.Once);
        _roleManagerMock.Verify(r => r.CreateAsync(It.Is<IdentityRole<Guid>>(role => role.Name == "Supervisor")), Times.Once);
        _roleManagerMock.Verify(r => r.CreateAsync(It.Is<IdentityRole<Guid>>(role => role.Name == "Admin")), Times.Once);
        _roleManagerMock.Verify(r => r.CreateAsync(It.IsAny<IdentityRole<Guid>>()), Times.Exactly(3));
    }

    [Fact]
    public async Task SeedRolesAsync_WhenRolesAlreadyExist_DoesNotRecreate() {
        _roleManagerMock.Setup(r => r.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        await RoleSeeder.SeedRolesAsync(_serviceProvider);

        _roleManagerMock.Verify(r => r.CreateAsync(It.IsAny<IdentityRole<Guid>>()), Times.Never);
    }

    public void Dispose() => _serviceProvider.Dispose();
}
