using System.ComponentModel.DataAnnotations;
using VocabMinerCourseWork.Api.Domains.Entities;

namespace VocabMinerCourseWork.Api.Domains.ViewModels;

public class ContentSourceRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required, MinLength(3)]
    public string Title { get; set; } = string.Empty;

    public ContentSourceType SourceType { get; set; } = ContentSourceType.PlainText;

    [Required, MinLength(10)]
    public string OriginalText { get; set; } = string.Empty;

    public string Language { get; set; } = "en";

    public string? Notes { get; set; }
}

public class ContentSourceUpdateRequest
{
    [Required, MinLength(3)]
    public string Title { get; set; } = string.Empty;

    public ContentSourceType SourceType { get; set; } = ContentSourceType.PlainText;

    [Required, MinLength(10)]
    public string OriginalText { get; set; } = string.Empty;

    public string Language { get; set; } = "en";

    public string? Notes { get; set; }
}

public class ContentSourceResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public ContentSourceType SourceType { get; set; }

    public string OriginalText { get; set; } = string.Empty;

    public string Language { get; set; } = string.Empty;

    public DateTime ImportedAt { get; set; }

    public int SegmentCount { get; set; }

    public string? Notes { get; set; }
}
