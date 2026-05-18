namespace VocabMinerCourseWork.Api.Domains.Entities;

public class ContentSource
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public ContentSourceType SourceType { get; set; } = ContentSourceType.PlainText;

    public string OriginalText { get; set; } = string.Empty;

    public string Language { get; set; } = "en";

    public DateTime ImportedAt { get; set; }

    public int SegmentCount { get; set; }

    public string? Notes { get; set; }

    public User? User { get; set; }

    public ICollection<Segment> Segments { get; set; } = new List<Segment>();

    public ICollection<Occurrence> Occurrences { get; set; } = new List<Occurrence>();
}
