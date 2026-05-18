using System.ComponentModel.DataAnnotations;
using VocabMinerCourseWork.Api.Domains.Entities;

namespace VocabMinerCourseWork.Api.Domains.ViewModels;

public class ExportCreateRequest
{
    [Required]
    public Guid UserId { get; set; }

    public ExportFormat Format { get; set; } = ExportFormat.Csv;

    public LearningStatus? FilterStatus { get; set; }
}

public class ExportJobResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public ExportFormat Format { get; set; }

    public string FileName { get; set; } = string.Empty;

    public ExportStatus Status { get; set; }

    public LearningStatus? FilterStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int RowCount { get; set; }
}
