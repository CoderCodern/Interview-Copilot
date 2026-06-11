using InterviewCopilot.Domain.Common;

namespace InterviewCopilot.Domain.Users;

// Domain events are past tense (CLAUDE.md §3). They are dispatched via the outbox on
// SaveChanges (Doc 01 §5) — e.g. UserRegistered triggers the verification email.

public sealed record UserRegistered(CandidateId UserId, string Email) : IDomainEvent;

public sealed record EmailVerified(CandidateId UserId) : IDomainEvent;

public sealed record UserLoggedIn(CandidateId UserId, DateTimeOffset At) : IDomainEvent;

public sealed record ExternalLoginLinked(CandidateId UserId, string Provider) : IDomainEvent;

public sealed record PasswordChanged(CandidateId UserId) : IDomainEvent;

public sealed record PasswordWasReset(CandidateId UserId) : IDomainEvent;

public sealed record AllSessionsRevoked(CandidateId UserId, string Reason) : IDomainEvent;

public sealed record OnboardingStepCompleted(CandidateId UserId, OnboardingStep Step) : IDomainEvent;

public sealed record OnboardingCompleted(CandidateId UserId) : IDomainEvent;
