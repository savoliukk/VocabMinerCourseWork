namespace VocabMinerCourseWork.Api.Domains.Entities;

public class Occurrence
{
    public Guid Id { get; set; }

    public Guid LearningUnitId { get; set; }

    public Guid SegmentId { get; set; }

    public Guid ContentSourceId { get; set; }

    public string? ContextBefore { get; set; }

    public string ContextText { get; set; } = string.Empty;

    public string? ContextAfter { get; set; }

    public int CharacterStart { get; set; }

    public int CharacterEnd { get; set; }

    public DateTime CreatedAt { get; set; }

    public LearningUnit? LearningUnit { get; set; }

    public Segment? Segment { get; set; }

    public ContentSource? ContentSource { get; set; }
}
