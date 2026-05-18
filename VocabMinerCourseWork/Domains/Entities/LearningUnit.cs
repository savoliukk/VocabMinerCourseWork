namespace VocabMinerCourseWork.Api.Domains.Entities;

public class LearningUnit
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Term { get; set; } = string.Empty;

    public string NormalizedTerm { get; set; } = string.Empty;

    public LearningUnitType UnitType { get; set; } = LearningUnitType.Word;

    public string? Translation { get; set; }

    public string? Explanation { get; set; }

    public string? ExampleSentence { get; set; }

    public LearningStatus Status { get; set; } = LearningStatus.New;

    public int Difficulty { get; set; } = 1;

    public DateTime ReviewDueAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User? User { get; set; }

    public ICollection<Occurrence> Occurrences { get; set; } = new List<Occurrence>();

    public ICollection<ReviewAttempt> ReviewAttempts { get; set; } = new List<ReviewAttempt>();
}
