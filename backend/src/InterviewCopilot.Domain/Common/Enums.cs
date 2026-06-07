namespace InterviewCopilot.Domain.Common;

/// <summary>Lifecycle of any analysis artifact (Doc 03, Doc 05 §1).</summary>
public enum AnalysisStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}

/// <summary>The kind of raw input feeding an analysis.</summary>
public enum SourceType
{
    Url = 0,
    Pdf = 1,
    Docx = 2,
    Image = 3,
    Text = 4
}

public enum QuestionCategory { Technical, Behavioral, SystemDesign, Cultural, Situational }

public enum Difficulty { Easy, Medium, Hard, Expert }

public enum GapSeverity { Minor, Moderate, Significant, Critical }
