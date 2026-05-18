using System.Text;
using Microsoft.AspNetCore.Mvc;
using VocabMinerCourseWork.Api.BusinessLogic;
using VocabMinerCourseWork.Api.Domains.Entities;
using VocabMinerCourseWork.Api.Domains.ViewModels;

namespace VocabMinerCourseWork.Api.Controllers;

[ApiController]
[Route("exports")]
public class ExportsController : ControllerBase
{
    private readonly IExportService _exportService;

    public ExportsController(IExportService exportService)
    {
        _exportService = exportService;
    }

    [HttpPost]
    public async Task<ActionResult<ExportJobResponse>> Create(ExportCreateRequest request, CancellationToken cancellationToken)
    {
        var job = await _exportService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = job.Id }, job);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ExportJobResponse>>> List(Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await _exportService.ListAsync(userId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExportJobResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var job = await _exportService.GetAsync(id, cancellationToken);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var download = await _exportService.DownloadAsync(id, cancellationToken);
        if (download is null)
        {
            return NotFound();
        }

        return File(Encoding.UTF8.GetBytes(download.Value.Payload), download.Value.ContentType, download.Value.FileName);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _exportService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("csv")]
    public Task<ActionResult<ExportJobResponse>> CreateCsv(ExportCreateRequest request, CancellationToken cancellationToken)
    {
        request.Format = ExportFormat.Csv;
        return Create(request, cancellationToken);
    }

    [HttpPost("tsv")]
    public Task<ActionResult<ExportJobResponse>> CreateTsv(ExportCreateRequest request, CancellationToken cancellationToken)
    {
        request.Format = ExportFormat.Tsv;
        return Create(request, cancellationToken);
    }

    [HttpGet("user/{userId:guid}/latest")]
    public async Task<ActionResult<IReadOnlyList<ExportJobResponse>>> Latest(Guid userId, [FromQuery] int take = 5, CancellationToken cancellationToken = default)
    {
        var jobs = await _exportService.ListAsync(userId, cancellationToken);
        return Ok(jobs.Take(Math.Clamp(take, 1, 50)).ToList());
    }

    [HttpGet("user/{userId:guid}/count")]
    public async Task<ActionResult<CountResponse>> Count(Guid userId, CancellationToken cancellationToken)
    {
        var jobs = await _exportService.ListAsync(userId, cancellationToken);
        return Ok(new CountResponse { Scope = "exports", Count = jobs.Count });
    }

    [HttpGet("user/{userId:guid}/completed")]
    public async Task<ActionResult<IReadOnlyList<ExportJobResponse>>> Completed(Guid userId, CancellationToken cancellationToken)
    {
        var jobs = await _exportService.ListAsync(userId, cancellationToken);
        return Ok(jobs.Where(job => job.Status == ExportStatus.Completed).ToList());
    }

    [HttpGet("{id:guid}/exists")]
    public async Task<ActionResult<ExistsResponse>> Exists(Guid id, CancellationToken cancellationToken)
    {
        var job = await _exportService.GetAsync(id, cancellationToken);
        return Ok(new ExistsResponse { Id = id, Exists = job is not null });
    }

    [HttpGet("{id:guid}/metadata")]
    public async Task<ActionResult<ExportMetadataResponse>> Metadata(Guid id, CancellationToken cancellationToken)
    {
        var job = await _exportService.GetAsync(id, cancellationToken);
        if (job is null)
        {
            return NotFound();
        }

        return Ok(ToMetadata(job));
    }

    [HttpGet("{id:guid}/preview")]
    public async Task<ActionResult<ExportPreviewResponse>> Preview(Guid id, [FromQuery] int lines = 5, CancellationToken cancellationToken = default)
    {
        var job = await _exportService.GetAsync(id, cancellationToken);
        var download = await _exportService.DownloadAsync(id, cancellationToken);
        if (job is null || download is null)
        {
            return NotFound();
        }

        return Ok(new ExportPreviewResponse
        {
            Id = job.Id,
            FileName = job.FileName,
            Lines = download.Value.Payload.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .Take(Math.Clamp(lines, 1, 50))
                .ToList()
        });
    }

    [HttpGet("{id:guid}/content-type")]
    public async Task<ActionResult<ApiMessageResponse>> ContentType(Guid id, CancellationToken cancellationToken)
    {
        var download = await _exportService.DownloadAsync(id, cancellationToken);
        return download is null
            ? NotFound()
            : Ok(new ApiMessageResponse { Message = download.Value.ContentType });
    }

    [HttpGet("formats")]
    public ActionResult<IReadOnlyList<EnumOptionResponse>> Formats()
    {
        return Ok(Enum.GetValues<ExportFormat>()
            .Select(value => new EnumOptionResponse { Name = value.ToString(), Value = (int)value })
            .ToList());
    }

    [HttpGet("statuses")]
    public ActionResult<IReadOnlyList<EnumOptionResponse>> Statuses()
    {
        return Ok(Enum.GetValues<ExportStatus>()
            .Select(value => new EnumOptionResponse { Name = value.ToString(), Value = (int)value })
            .ToList());
    }

    [HttpPost("validate")]
    public ActionResult<ValidationResponse> Validate(ExportCreateRequest request)
    {
        return Ok(new ValidationResponse { IsValid = ModelState.IsValid });
    }

    private static ExportMetadataResponse ToMetadata(ExportJobResponse job)
    {
        return new ExportMetadataResponse
        {
            Id = job.Id,
            FileName = job.FileName,
            Format = job.Format,
            Status = job.Status,
            RowCount = job.RowCount,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt,
            ContentType = job.Format == ExportFormat.Tsv ? "text/tab-separated-values" : "text/csv"
        };
    }
}
