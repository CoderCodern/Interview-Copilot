namespace InterviewCopilot.Domain.Resumes;

public enum SkillCategory { Language, Framework, Tool, Platform, Database, Soft, Domain, Other }

public enum ProficiencyLevel { Beginner, Intermediate, Advanced, Expert }

/// <summary>Structured contact block (kept minimal; PII handled per Doc 10 §5).</summary>
public sealed record ContactInfo(string? FullName, string? Email, string? Phone, string? Location, string? LinkedInUrl);

public sealed record Skill(string Name, SkillCategory Category, ProficiencyLevel? Level = null, int? YearsOfExperience = null);

public sealed record ExperienceItem(
    string Title,
    string Company,
    DateOnly? StartDate,
    DateOnly? EndDate,
    IReadOnlyList<string> Achievements,
    IReadOnlyList<string> Technologies);

public sealed record ProjectItem(string Name, string? Description, IReadOnlyList<string> Technologies, string? Url);

public sealed record EducationItem(string Institution, string? Degree, string? FieldOfStudy, int? GraduationYear);

public sealed record Certification(string Name, string? Issuer, int? Year);

/// <summary>The structured output of parsing a resume (Doc 03 §4.2).</summary>
public sealed record ResumeProfile(
    ContactInfo Contact,
    IReadOnlyList<Skill> Skills,
    IReadOnlyList<ExperienceItem> Experience,
    IReadOnlyList<ProjectItem> Projects,
    IReadOnlyList<EducationItem> Education,
    IReadOnlyList<Certification> Certifications,
    string? Summary);
