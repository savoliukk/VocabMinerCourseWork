using System.ComponentModel.DataAnnotations;
using VocabMinerCourseWork.Api.Domains.Entities;

namespace VocabMinerCourseWork.Api.Domains.ViewModels;

public class ReviewSubmitRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid LearningUnitId { get; set; }

    public ReviewGrade Grade { get; set; } = ReviewGrade.Good;

    [Range(0, 3600)]
    public int ResponseTimeSeconds { get; set; }

    public string? Notes { get; set; }
}

public class ReviewResponse
{
    public Guid LearningUnitId { get; set; }

    public string Term { get; set; } = string.Empty;

    public string? Translation { get; set; }

    public string? Explanation { get; set; }

    public string? ExampleSentence { get; set; }

    public LearningStatus Status { get; set; }

    public DateTime ReviewDueAt { get; set; }
}

public class ReviewAttemptResponse
{
    public Guid Id { get; set; }

    public Guid LearningUnitId { get; set; }

    public ReviewGrade Grade { get; set; }

    public int ResponseTimeSeconds { get; set; }

    public DateTime ReviewedAt { get; set; }

    public DateTime NextDueAt { get; set; }

    public string? Notes { get; set; }
}
