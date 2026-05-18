namespace VocabMinerCourseWork.Api.Domains.Entities;

public class ReviewAttempt
{
    public Guid Id { get; set; }

    public Guid LearningUnitId { get; set; }

    public Guid UserId { get; set; }

    public DateTime ReviewedAt { get; set; }

    public ReviewGrade Grade { get; set; }

    public int ResponseTimeSeconds { get; set; }

    public DateTime NextDueAt { get; set; }

    public string? Notes { get; set; }

    public LearningUnit? LearningUnit { get; set; }

    public User? User { get; set; }
}
