using FluentAssertions;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Resumes;
using Xunit;

namespace InterviewCopilot.Domain.UnitTests;

public class ResumeTests
{
    private static ResumeProfile SampleProfile() => new(
        new ContactInfo("Ada Lovelace", "ada@example.com", null, "London", null),
        Skills: [new Skill("C#", SkillCategory.Language, ProficiencyLevel.Expert, 8)],
        Experience: [], Projects: [], Education: [], Certifications: [], Summary: null);

    [Fact]
    public void Upload_raises_ResumeUploaded_and_starts_pending()
    {
        var owner = CandidateId.New();
        var source = AnalysisSource.FromText("plain resume text").Value;

        var resume = Resume.Upload(owner, source);

        resume.Status.Should().Be(AnalysisStatus.Pending);
        resume.OwnerId.Should().Be(owner);
        resume.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<ResumeUploaded>();
    }

    [Fact]
    public void Complete_requires_processing_state()
    {
        var resume = Resume.Upload(CandidateId.New(), AnalysisSource.FromText("x").Value);

        // Cannot complete straight from Pending.
        resume.Complete(SampleProfile()).IsFailure.Should().BeTrue();

        resume.StartProcessing();
        var result = resume.Complete(SampleProfile());

        result.IsSuccess.Should().BeTrue();
        resume.Status.Should().Be(AnalysisStatus.Completed);
        resume.Profile.Should().NotBeNull();
        resume.DomainEvents.Should().ContainItemsAssignableTo<ResumeParsed>();
    }

    [Fact]
    public void Completed_resume_cannot_fail()
    {
        var resume = Resume.Upload(CandidateId.New(), AnalysisSource.FromText("x").Value);
        resume.StartProcessing();
        resume.Complete(SampleProfile());

        resume.Fail("nope").IsFailure.Should().BeTrue();
        resume.Status.Should().Be(AnalysisStatus.Completed);
    }

    [Fact]
    public void Invalid_url_source_fails_validation()
    {
        AnalysisSource.FromUrl("not-a-url").IsFailure.Should().BeTrue();
        AnalysisSource.FromUrl("https://acme.com/about").IsSuccess.Should().BeTrue();
    }
}
