using System.ComponentModel.DataAnnotations;
using Dyadic.Web.Pages.Student;
using FluentAssertions;

namespace Dyadic.UnitTests.Validators;

using InputModel = SubmitProposalModel.InputModel;

public class ProposalInputValidatorTests
{
    // ── helper ─────────────────────────────────────────────────────────────────

    private static IList<ValidationResult> Validate(InputModel input)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(input);
        Validator.TryValidateObject(input, context, results, validateAllProperties: true);
        return results;
    }

    private static InputModel ValidInput() => new()
    {
        Title = "Applying ML to Cybersecurity",
        Abstract = new string('A', 50),  // exactly 50 chars — meets MinLength
        TechStack = "C#, ASP.NET Core",
        ResearchAreaId = Guid.NewGuid()
    };

    // ── positive tests ─────────────────────────────────────────────────────────

    [Fact]
    public void ValidTitle_AlphanumericPunctuation_PassesValidation()
    {
        var input = ValidInput();
        input.Title = "Machine Learning: A Case Study (2024)";

        var errors = Validate(input);

        errors.Should().NotContain(r => r.MemberNames.Contains(nameof(InputModel.Title)));
    }

    [Fact]
    public void ValidAbstract_WithNewlines_PassesValidation()
    {
        var input = ValidInput();
        input.Abstract = "This proposal explores machine learning.\r\nIt covers several topics in depth.";

        var errors = Validate(input);

        errors.Should().NotContain(r => r.MemberNames.Contains(nameof(InputModel.Abstract)));
    }

    [Fact]
    public void ValidTechStack_WithSpecialChars_PassesValidation()
    {
        var input = ValidInput();
        input.TechStack = "C#, ASP.NET Core, EF Core, Python/scikit-learn, C++";

        var errors = Validate(input);

        errors.Should().NotContain(r => r.MemberNames.Contains(nameof(InputModel.TechStack)));
    }

    [Fact]
    public void ValidResearchAreaId_NonEmptyGuid_PassesValidation()
    {
        var input = ValidInput();
        input.ResearchAreaId = Guid.NewGuid();

        var errors = Validate(input);

        errors.Should().NotContain(r => r.MemberNames.Contains(nameof(InputModel.ResearchAreaId)));
    }

    // ── negative tests ─────────────────────────────────────────────────────────

    [Fact]
    public void Title_WithHtmlScriptTag_FailsRegex()
    {
        var input = ValidInput();
        input.Title = "<script>alert(1)</script>";

        var errors = Validate(input);

        errors.Should().Contain(r => r.MemberNames.Contains(nameof(InputModel.Title)));
    }

    [Fact]
    public void Title_WithEmoji_FailsRegex()
    {
        var input = ValidInput();
        input.Title = "Proposal with Emoji \U0001F680";

        var errors = Validate(input);

        errors.Should().Contain(r => r.MemberNames.Contains(nameof(InputModel.Title)));
    }

    [Fact]
    public void Abstract_WithControlCharacter_FailsRegex()
    {
        var input = ValidInput();
        input.Abstract = new string('A', 50) + "\x01\x02";

        var errors = Validate(input);

        errors.Should().Contain(r => r.MemberNames.Contains(nameof(InputModel.Abstract)));
    }

    [Fact]
    public void Title_AllWhitespace_FailsRequired()
    {
        var input = ValidInput();
        input.Title = "          ";

        var errors = Validate(input);

        errors.Should().Contain(r => r.MemberNames.Contains(nameof(InputModel.Title)));
    }

    [Fact]
    public void Title_TooShort_FailsMinLength()
    {
        var input = ValidInput();
        input.Title = "Hi";

        var errors = Validate(input);

        errors.Should().Contain(r => r.MemberNames.Contains(nameof(InputModel.Title)));
    }

    [Fact]
    public void Abstract_TooShort_FailsMinLength()
    {
        var input = ValidInput();
        input.Abstract = "Too short";

        var errors = Validate(input);

        errors.Should().Contain(r => r.MemberNames.Contains(nameof(InputModel.Abstract)));
    }

    [Fact]
    public void ResearchAreaId_Null_FailsRequired()
    {
        var input = ValidInput();
        input.ResearchAreaId = null;

        var errors = Validate(input);

        errors.Should().Contain(r => r.MemberNames.Contains(nameof(InputModel.ResearchAreaId)));
    }

    [Fact]
    public void TechStack_WithAngleBrackets_FailsRegex()
    {
        var input = ValidInput();
        input.TechStack = "<bad>stack</bad>";

        var errors = Validate(input);

        errors.Should().Contain(r => r.MemberNames.Contains(nameof(InputModel.TechStack)));
    }

    // ── edge-case tests ────────────────────────────────────────────────────────

    [Fact]
    public void Title_ExactlyMinLength10_PassesValidation()
    {
        var input = ValidInput();
        input.Title = "1234567890"; // exactly 10 chars

        var errors = Validate(input);

        errors.Should().NotContain(r => r.MemberNames.Contains(nameof(InputModel.Title)));
    }

    [Fact]
    public void Title_ExactlyMaxLength200_PassesValidation()
    {
        var input = ValidInput();
        input.Title = new string('A', 200);

        var errors = Validate(input);

        errors.Should().NotContain(r => r.MemberNames.Contains(nameof(InputModel.Title)));
    }

    [Fact]
    public void Abstract_ExactlyMinLength50_PassesValidation()
    {
        var input = ValidInput();
        input.Abstract = new string('A', 50);

        var errors = Validate(input);

        errors.Should().NotContain(r => r.MemberNames.Contains(nameof(InputModel.Abstract)));
    }
}
