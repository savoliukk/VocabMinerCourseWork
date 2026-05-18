using System.Text;
using VocabMinerCourseWork.Api.Domains.Entities;
using VocabMinerCourseWork.Api.Domains.ViewModels;
using VocabMinerCourseWork.Api.Repositories;

namespace VocabMinerCourseWork.Api.BusinessLogic;

public interface IExportService
{
    Task<ExportJobResponse> CreateAsync(ExportCreateRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExportJobResponse>> ListAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ExportJobResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(string FileName, string ContentType, string Payload)?> DownloadAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class ExportService : IExportService
{
    private readonly IExportRepository _exportRepository;
    private readonly ILearningUnitRepository _learningUnitRepository;

    public ExportService(IExportRepository exportRepository, ILearningUnitRepository learningUnitRepository)
    {
        _exportRepository = exportRepository;
        _learningUnitRepository = learningUnitRepository;
    }

    public async Task<ExportJobResponse> CreateAsync(ExportCreateRequest request, CancellationToken cancellationToken = default)
    {
        var units = await _learningUnitRepository.ListAsync(request.UserId, request.FilterStatus, null, cancellationToken);
        var payload = BuildDelimitedExport(units, request.Format);
        var extension = request.Format == ExportFormat.Tsv ? "tsv" : "csv";
        var now = DateTime.UtcNow;
        var job = new ExportJob
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Format = request.Format,
            FileName = $"vocabminer-{now:yyyyMMddHHmmss}.{extension}",
            Status = ExportStatus.Completed,
            FilterStatus = request.FilterStatus,
            CreatedAt = now,
            CompletedAt = now,
            RowCount = units.Count,
            Payload = payload
        };

        await _exportRepository.AddAsync(job, cancellationToken);
        return ToResponse(job);
    }

    public async Task<IReadOnlyList<ExportJobResponse>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var jobs = await _exportRepository.ListByUserAsync(userId, cancellationToken);
        return jobs.Select(ToResponse).ToList();
    }

    public async Task<ExportJobResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _exportRepository.GetByIdAsync(id, cancellationToken);
        return job is null ? null : ToResponse(job);
    }

    public async Task<(string FileName, string ContentType, string Payload)?> DownloadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _exportRepository.GetByIdAsync(id, cancellationToken);
        if (job is null)
        {
            return null;
        }

        var contentType = job.Format == ExportFormat.Tsv ? "text/tab-separated-values" : "text/csv";
        return (job.FileName, contentType, job.Payload);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _exportRepository.DeleteAsync(id, cancellationToken);
    }

    public static string BuildDelimitedExport(IEnumerable<LearningUnit> units, ExportFormat format)
    {
        var delimiter = format == ExportFormat.Tsv ? '\t' : ',';
        var builder = new StringBuilder();
        builder.AppendJoin(delimiter, "Term", "Translation", "Explanation", "Example", "Status");
        builder.AppendLine();

        foreach (var unit in units)
        {
            builder.AppendJoin(delimiter,
                Escape(unit.Term, delimiter),
                Escape(unit.Translation ?? string.Empty, delimiter),
                Escape(unit.Explanation ?? string.Empty, delimiter),
                Escape(unit.ExampleSentence ?? string.Empty, delimiter),
                unit.Status.ToString());
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string Escape(string value, char delimiter)
    {
        if (delimiter == '\t')
        {
            return value.Replace('\t', ' ').ReplaceLineEndings(" ");
        }

        var sanitized = value.ReplaceLineEndings(" ");
        return sanitized.Contains(',') || sanitized.Contains('"')
            ? $"\"{sanitized.Replace("\"", "\"\"")}\""
            : sanitized;
    }

    private static ExportJobResponse ToResponse(ExportJob job)
    {
        return new ExportJobResponse
        {
            Id = job.Id,
            UserId = job.UserId,
            Format = job.Format,
            FileName = job.FileName,
            Status = job.Status,
            FilterStatus = job.FilterStatus,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt,
            RowCount = job.RowCount
        };
    }
}
