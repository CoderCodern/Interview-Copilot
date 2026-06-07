using InterviewCopilot.Application.Features.Resumes.GetResume;
using InterviewCopilot.Application.Features.Resumes.UploadResume;
using MediatR;

namespace InterviewCopilot.Api.Endpoints;

/// <summary>Resume resource group (Doc 05 §2). Thin: map route → command/query → Result→HTTP.</summary>
public static class ResumeEndpoints
{
    public static RouteGroupBuilder MapResumeEndpoints(this RouteGroupBuilder group)
    {
        var resumes = group.MapGroup("/resumes").WithTags("Resumes");

        resumes.MapPost("/", async (UploadResumeCommand command, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(command, ct);
            return result.ToCreated(r => $"/api/v1/resumes/{r.Id}");
        })
        .WithName("UploadResume")
        .WithSummary("Register a resume source and start async parsing.");

        resumes.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetResumeQuery(id), ct);
            return result.ToOk();
        })
        .WithName("GetResume");

        return group;
    }
}
