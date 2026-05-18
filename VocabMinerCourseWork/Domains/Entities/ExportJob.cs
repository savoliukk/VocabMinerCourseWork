namespace VocabMinerCourseWork.Api.Domains.Entities;

public class ExportJob
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public ExportFormat Format { get; set; } = ExportFormat.Csv;

    public string FileName { get; set; } = string.Empty;

    public ExportStatus Status { get; set; } = ExportStatus.Pending;

    public LearningStatus? FilterStatus { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int RowCount { get; set; }

    public string Payload { get; set; } = string.Empty;

    public User? User { get; set; }
}
