using System.ComponentModel.DataAnnotations;
using VocabMinerCourseWork.Api.Domains.Entities;

namespace VocabMinerCourseWork.Api.Domains.ViewModels;

public class LearningUnitRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required, MinLength(1)]
    public string Term { get; set; } = string.Empty;

    public LearningUnitType UnitType { get; set; } = LearningUnitType.Word;

    public string? Translation { get; set; }

    public string? Explanation { get; set; }

    public Guid? SegmentId { get; set; }
}

public class LearningUnitUpdateRequest
{
    [Required, MinLength(1)]
    public string Term { get; set; } = string.Empty;

    public LearningUnitType UnitType { get; set; } = LearningUnitType.Word;

    public string? Translation { get; set; }

    public string? Explanation { get; set; }

    public string? ExampleSentence { get; set; }

    public LearningStatus Status { get; set; } = LearningStatus.New;

    [Range(1, 5)]
    public int Difficulty { get; set; } = 1;
}

public class PromotePhraseRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid SegmentId { get; set; }

    [Required, MinLength(2)]
    public string Phrase { get; set; } = string.Empty;
}

public class LearningUnitResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Term { get; set; } = string.Empty;

    public string NormalizedTerm { get; set; } = string.Empty;

    public LearningUnitType UnitType { get; set; }

    public string? Translation { get; set; }

    public string? Explanation { get; set; }

    public string? ExampleSentence { get; set; }

    public LearningStatus Status { get; set; }

    public int Difficulty { get; set; }

    public DateTime ReviewDueAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int OccurrenceCount { get; set; }
}

public class ExplanationResponse
{
    public Guid LearningUnitId { get; set; }

    public string Term { get; set; } = string.Empty;

    public string Translation { get; set; } = string.Empty;

    public string Explanation { get; set; } = string.Empty;

    public string ExampleSentence { get; set; } = string.Empty;
}
