using FluentAssertions;
using InterviewCopilot.Domain.Common;
using InterviewCopilot.Domain.Users;
using Xunit;

namespace InterviewCopilot.Domain.UnitTests.Users;

public class UserSessionTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 10, 12, 0, 0, TimeSpan.Zero);

    private static UserSession StartSession(string hash = "hash-1") =>
        UserSession.Start(CandidateId.New(), hash, Now.AddDays(30), Now, "ua", "ip");

    [Fact]
    public void Start_issues_one_active_token()
    {
        var session = StartSession();

        session.IsActive.Should().BeTrue();
        session.Tokens.Should().ContainSingle();
        session.Tokens[0].IsActive(Now).Should().BeTrue();
    }

    [Fact]
    public void Rotate_revokes_old_and_issues_new_linked_token()
    {
        var session = StartSession("old");

        var result = session.Rotate("old", "new", Now.AddDays(30), Now.AddMinutes(5));

        result.IsSuccess.Should().BeTrue();
        var oldToken = session.Tokens.Single(t => t.TokenHash == "old");
        var newToken = session.Tokens.Single(t => t.TokenHash == "new");
        oldToken.IsRevoked.Should().BeTrue();
        oldToken.ReplacedById.Should().Be(newToken.Id);
        newToken.IsActive(Now.AddMinutes(5)).Should().BeTrue();
    }

    [Fact]
    public void Rotate_with_unknown_token_fails_without_revoking_session()
    {
        var session = StartSession("old");

        var result = session.Rotate("does-not-exist", "new", Now.AddDays(30), Now);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("auth.refresh_invalid");
        session.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Reusing_a_rotated_token_revokes_the_whole_session()
    {
        var session = StartSession("old");
        session.Rotate("old", "new", Now.AddDays(30), Now.AddMinutes(5)); // "old" now revoked

        // Attacker replays the already-rotated "old" token.
        var replay = session.Rotate("old", "evil", Now.AddDays(30), Now.AddMinutes(10));

        replay.IsFailure.Should().BeTrue();
        replay.Error.Code.Should().Be("auth.refresh_reused");
        session.IsActive.Should().BeFalse();
        session.Tokens.Should().OnlyContain(t => t.IsRevoked);
    }

    [Fact]
    public void Expired_token_cannot_be_rotated()
    {
        var session = UserSession.Start(CandidateId.New(), "old", Now.AddMinutes(1), Now);

        var result = session.Rotate("old", "new", Now.AddDays(30), Now.AddMinutes(2));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("auth.refresh_reused");
        session.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Revoke_marks_session_and_tokens_revoked()
    {
        var session = StartSession();

        session.Revoke(Now.AddHours(1));

        session.IsActive.Should().BeFalse();
        session.Tokens.Should().OnlyContain(t => t.IsRevoked);
    }
}
