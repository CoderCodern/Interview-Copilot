using InterviewCopilot.Domain.Common;

namespace InterviewCopilot.Domain.Resumes;

public sealed record ResumeUploaded(ResumeId ResumeId, CandidateId OwnerId) : IDomainEvent;

public sealed record ResumeParsed(ResumeId ResumeId, CandidateId OwnerId) : IDomainEvent;

public sealed record ResumeParseFailed(ResumeId ResumeId, CandidateId OwnerId, string Reason) : IDomainEvent;
