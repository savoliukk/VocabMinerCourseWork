namespace VocabMinerCourseWork.Api.Domains.Entities;

public class Segment
{
    public Guid Id { get; set; }

    public Guid ContentSourceId { get; set; }

    public int Position { get; set; }

    public string Text { get; set; } = string.Empty;

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public ContentSource? ContentSource { get; set; }

    public ICollection<Occurrence> Occurrences { get; set; } = new List<Occurrence>();
}
