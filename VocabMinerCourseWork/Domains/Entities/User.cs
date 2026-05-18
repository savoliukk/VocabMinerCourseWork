namespace VocabMinerCourseWork.Api.Domains.Entities;

public class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string NativeLanguage { get; set; } = "uk";

    public string TargetLanguage { get; set; } = "en";

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ContentSource> ContentSources { get; set; } = new List<ContentSource>();

    public ICollection<LearningUnit> LearningUnits { get; set; } = new List<LearningUnit>();

    public ICollection<ReviewAttempt> ReviewAttempts { get; set; } = new List<ReviewAttempt>();

    public ICollection<ExportJob> ExportJobs { get; set; } = new List<ExportJob>();
}
