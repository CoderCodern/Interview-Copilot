using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace InterviewCopilot.Architecture.Tests;

/// <summary>
/// Executable enforcement of the Clean Architecture dependency rule (Doc 01 §2). These run in CI
/// and fail the build if a layer reaches outward.
/// </summary>
public class DependencyRuleTests
{
    private const string Domain = "InterviewCopilot.Domain";
    private const string Application = "InterviewCopilot.Application";
    private const string Infrastructure = "InterviewCopilot.Infrastructure";

    private static readonly System.Reflection.Assembly DomainAsm = typeof(Domain.Common.Entity<>).Assembly;
    private static readonly System.Reflection.Assembly ApplicationAsm = typeof(Application.DependencyInjection).Assembly;

    [Fact]
    public void Domain_should_not_depend_on_any_other_layer()
    {
        var result = Types.InAssembly(DomainAsm)
            .ShouldNot()
            .HaveDependencyOnAny(Application, Infrastructure)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "the domain must remain dependency-free (Doc 01 §2)");
    }

    [Fact]
    public void Application_should_not_depend_on_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAsm)
            .ShouldNot()
            .HaveDependencyOn(Infrastructure)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "the application defines ports; infrastructure provides adapters, not the reverse");
    }

    [Fact]
    public void Command_and_query_handlers_should_be_sealed()
    {
        var result = Types.InAssembly(ApplicationAsm)
            .That().HaveNameEndingWith("Handler")
            .Should().BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
