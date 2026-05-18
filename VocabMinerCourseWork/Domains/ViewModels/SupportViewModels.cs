using System.ComponentModel.DataAnnotations;
using VocabMinerCourseWork.Api.Domains.Entities;

namespace VocabMinerCourseWork.Api.Domains.ViewModels;

public class ApiMessageResponse
{
    public string Message { get; set; } = string.Empty;

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class CountResponse
{
    public string Scope { get; set; } = string.Empty;

    public int Count { get; set; }
}

public class ExistsResponse
{
    public Guid Id { get; set; }

    public bool Exists { get; set; }
}

public class ValidationResponse
{
    public bool IsValid { get; set; }

    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();
}

public class EnumOptionResponse
{
    public string Name { get; set; } = string.Empty;

    public int Value { get; set; }
}

public class EmailAvailabilityRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class EmailAvailabilityResponse
{
    public string Email { get; set; } = string.Empty;

    public bool IsAvailable { get; set; }
}

public class ProfileLookupRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}

public class RegisterDefaultsResponse
{
    public string NativeLanguage { get; set; } = "uk";

    public string TargetLanguage { get; set; } = "en";

    public int MinPasswordLength { get; set; } = 6;
}

public class UserStatusResponse
{
    public Guid UserId { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastLoginAt { get; set; }
}

public class UserLanguageResponse
{
    public Guid UserId { get; set; }

    public string NativeLanguage { get; set; } = string.Empty;

    public string TargetLanguage { get; set; } = string.Empty;
}

public class UserAuditResponse
{
    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; }
}

public class TextSegmentationRequest
{
    [Required, MinLength(1)]
    public string Text { get; set; } = string.Empty;

    [Range(1, 50)]
    public int MaxSegments { get; set; } = 10;
}

public class SegmentPreviewResponse
{
    public int SegmentCount { get; set; }

    public IReadOnlyList<string> Segments { get; set; } = Array.Empty<string>();
}

public class ContentSourceSummaryResponse
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public ContentSourceType SourceType { get; set; }

    public string Language { get; set; } = string.Empty;

    public int SegmentCount { get; set; }

    public DateTime ImportedAt { get; set; }

    public int TextLength { get; set; }
}

public class TextPreviewResponse
{
    public Guid Id { get; set; }

    public string Preview { get; set; } = string.Empty;

    public int Length { get; set; }
}

public class SegmentContextResponse
{
    public Guid SegmentId { get; set; }

    public int Position { get; set; }

    public string? PreviousText { get; set; }

    public string CurrentText { get; set; } = string.Empty;

    public string? NextText { get; set; }
}

public class SegmentLengthResponse
{
    public Guid SegmentId { get; set; }

    public int CharacterCount { get; set; }

    public int WordCount { get; set; }
}

public class SegmentPositionsResponse
{
    public Guid ContentSourceId { get; set; }

    public IReadOnlyList<int> Positions { get; set; } = Array.Empty<int>();
}

public class NormalizedTextResponse
{
    public string OriginalText { get; set; } = string.Empty;

    public string NormalizedText { get; set; } = string.Empty;
}

public class LearningUnitNormalizeRequest
{
    [Required, MinLength(1)]
    public string Term { get; set; } = string.Empty;
}

public class LearningUnitNormalizeResponse
{
    public string Term { get; set; } = string.Empty;

    public string NormalizedTerm { get; set; } = string.Empty;
}

public class LearningUnitReviewStateResponse
{
    public Guid LearningUnitId { get; set; }

    public LearningStatus Status { get; set; }

    public int Difficulty { get; set; }

    public DateTime ReviewDueAt { get; set; }

    public bool IsDue { get; set; }
}

public class ReviewQuickSubmitRequest
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid LearningUnitId { get; set; }

    [Range(0, 3600)]
    public int ResponseTimeSeconds { get; set; }

    public string? Notes { get; set; }
}

public class ReviewStatsResponse
{
    public Guid UserId { get; set; }

    public int DueCount { get; set; }

    public int HistoryCount { get; set; }

    public int AgainCount { get; set; }

    public int HardCount { get; set; }

    public int GoodCount { get; set; }

    public int EasyCount { get; set; }
}

public class ExportMetadataResponse
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public ExportFormat Format { get; set; }

    public ExportStatus Status { get; set; }

    public int RowCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string ContentType { get; set; } = string.Empty;
}

public class ExportPreviewResponse
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public IReadOnlyList<string> Lines { get; set; } = Array.Empty<string>();
}
