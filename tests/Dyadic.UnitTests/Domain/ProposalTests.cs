using Dyadic.Domain.Entities;
using Dyadic.Domain.Enums;
using FluentAssertions;

namespace Dyadic.UnitTests.Domain;

public class ProposalTests {
    [Fact]
    public void Status_ShouldDefaultToDraft() {
        var proposal = new Proposal {
            Title = "Test Proposal",
            Abstract = "A test"
        };

        proposal.Status.Should().Be(ProposalStatus.Draft);
    }

    [Fact]
    public void CreatedAt_ShouldDefaultToUtcNow() {
        var before = DateTime.UtcNow;
        var proposal = new Proposal {
            Title = "Test",
            Abstract = "Test"
        };
        var after = DateTime.UtcNow;

        proposal.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void SupervisorId_ShouldBeNullByDefault() {
        var proposal = new Proposal {
            Title = "Test",
            Abstract = "Test"
        };

        proposal.SupervisorId.Should().BeNull();
        proposal.Supervisor.Should().BeNull();
    }
}