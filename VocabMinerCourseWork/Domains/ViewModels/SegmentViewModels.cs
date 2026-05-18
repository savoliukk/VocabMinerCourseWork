using System.ComponentModel.DataAnnotations;

namespace VocabMinerCourseWork.Api.Domains.ViewModels;

public class SegmentUpdateRequest
{
    [Required, MinLength(1)]
    public string Text { get; set; } = string.Empty;
}

public class SegmentResponse
{
    public Guid Id { get; set; }

    public Guid ContentSourceId { get; set; }

    public int Position { get; set; }

    public string Text { get; set; } = string.Empty;

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    public DateTime CreatedAt { get; set; }
}
