using InterviewCopilot.Application.Abstractions;
using InterviewCopilot.Application.Common.Messaging;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Resumes;
using FluentValidation;

namespace InterviewCopilot.Application.Features.Resumes.UploadResume;

// ---- Response DTO -------------------------------------------------------------
public sealed record ResumeResponse(Guid Id, string Status, DateTimeOffset CreatedAt);

// ---- Command ------------------------------------------------------------------
/// <summary>Registers a new resume source and kicks off async parsing (Doc 05 §3, §4).</summary>
public sealed record UploadResumeCommand(
    SourceType SourceType,
    string? Text,
    string? BlobKey,
    string? ContentType,
    string? Checksum) : ICommand<ResumeResponse>, ITransactional;

// ---- Validator ----------------------------------------------------------------
public sealed class UploadResumeValidator : AbstractValidator<UploadResumeCommand>
{
    public UploadResumeValidator()
    {
        When(c => c.SourceType == SourceType.Text, () =>
            RuleFor(c => c.Text).NotEmpty().WithMessage("Text is required for a text source."));

        When(c => c.SourceType is SourceType.Pdf or SourceType.Docx or SourceType.Image, () =>
        {
            RuleFor(c => c.BlobKey).NotEmpty().WithMessage("A blobKey from /uploads is required.");
            RuleFor(c => c.ContentType).NotEmpty();
        });
    }
}

// ---- Handler ------------------------------------------------------------------
public sealed class UploadResumeHandler(IResumeRepository repository, ICurrentUser currentUser)
    : ICommandHandler<UploadResumeCommand, ResumeResponse>
{
    public async Task<Result<ResumeResponse>> Handle(UploadResumeCommand cmd, CancellationToken ct)
    {
        Result<AnalysisSource> source = cmd.SourceType switch
        {
            SourceType.Text => AnalysisSource.FromText(cmd.Text!),
            SourceType.Pdf or SourceType.Docx or SourceType.Image =>
                BuildBlobSource(cmd),
            _ => Error.Validation("resume.unsupported_source", "Unsupported source type for resume.")
        };

        if (source.IsFailure) return source.Error;

        var resume = Resume.Upload(currentUser.Id, source.Value);
        repository.Add(resume);

        // UnitOfWorkBehavior commits + dispatches ResumeUploaded → worker parses async.
        return new ResumeResponse(resume.Id.Value, resume.Status.ToString(), resume.CreatedAt);
    }

    private static Result<AnalysisSource> BuildBlobSource(UploadResumeCommand cmd)
    {
        var blob = BlobReference.Create(cmd.BlobKey!, cmd.ContentType!, cmd.Checksum);
        return blob.IsFailure ? blob.Error : AnalysisSource.FromBlob(cmd.SourceType, blob.Value);
    }
}
