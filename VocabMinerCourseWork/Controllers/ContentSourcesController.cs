using Microsoft.AspNetCore.Mvc;
using VocabMinerCourseWork.Api.BusinessLogic;
using VocabMinerCourseWork.Api.Domains.Entities;
using VocabMinerCourseWork.Api.Domains.ViewModels;

namespace VocabMinerCourseWork.Api.Controllers;

[ApiController]
[Route("content-sources")]
public class ContentSourcesController : ControllerBase
{
    private readonly IContentImportService _contentImportService;

    public ContentSourcesController(IContentImportService contentImportService)
    {
        _contentImportService = contentImportService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ContentSourceResponse>>> List([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await _contentImportService.ListSourcesAsync(userId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContentSourceResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var source = await _contentImportService.GetSourceAsync(id, cancellationToken);
        return source is null ? NotFound() : Ok(source);
    }

    [HttpPost]
    public async Task<ActionResult<ContentSourceResponse>> Create(ContentSourceRequest request, CancellationToken cancellationToken)
    {
        var source = await _contentImportService.ImportAsync(request, cancellationToken);
        if (source is null)
        {
            return NotFound(new ApiMessageResponse { Message = $"User '{request.UserId}' was not found." });
        }

        return CreatedAtAction(nameof(Get), new { id = source.Id }, source);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ContentSourceResponse>> Update(Guid id, ContentSourceUpdateRequest request, CancellationToken cancellationToken)
    {
        var source = await _contentImportService.UpdateSourceAsync(id, request, cancellationToken);
        return source is null ? NotFound() : Ok(source);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _contentImportService.DeleteSourceAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("recent")]
    public async Task<ActionResult<IReadOnlyList<ContentSourceResponse>>> Recent([FromQuery] Guid userId, [FromQuery] int take = 5, CancellationToken cancellationToken = default)
    {
        var sources = await _contentImportService.ListSourcesAsync(userId, cancellationToken);
        return Ok(sources.Take(Math.Clamp(take, 1, 50)).ToList());
    }

    [HttpGet("count")]
    public async Task<ActionResult<CountResponse>> Count([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        var sources = await _contentImportService.ListSourcesAsync(userId, cancellationToken);
        return Ok(new CountResponse { Scope = "content-sources", Count = sources.Count });
    }

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<ContentSourceResponse>>> Search([FromQuery] Guid userId, [FromQuery] string term, CancellationToken cancellationToken)
    {
        var sources = await _contentImportService.ListSourcesAsync(userId, cancellationToken);
        var safeTerm = term ?? string.Empty;
        var filtered = sources
            .Where(source => source.Title.Contains(safeTerm, StringComparison.OrdinalIgnoreCase) ||
                             source.OriginalText.Contains(safeTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();
        return Ok(filtered);
    }

    [HttpGet("by-type")]
    public async Task<ActionResult<IReadOnlyList<ContentSourceResponse>>> ByType([FromQuery] Guid userId, [FromQuery] ContentSourceType sourceType, CancellationToken cancellationToken)
    {
        var sources = await _contentImportService.ListSourcesAsync(userId, cancellationToken);
        return Ok(sources.Where(source => source.SourceType == sourceType).ToList());
    }

    [HttpGet("languages")]
    public async Task<ActionResult<IReadOnlyList<string>>> Languages([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        var sources = await _contentImportService.ListSourcesAsync(userId, cancellationToken);
        return Ok(sources.Select(source => source.Language).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(language => language).ToList());
    }

    [HttpGet("{id:guid}/summary")]
    public async Task<ActionResult<ContentSourceSummaryResponse>> Summary(Guid id, CancellationToken cancellationToken)
    {
        var source = await _contentImportService.GetSourceAsync(id, cancellationToken);
        if (source is null)
        {
            return NotFound();
        }

        return Ok(new ContentSourceSummaryResponse
        {
            Id = source.Id,
            Title = source.Title,
            SourceType = source.SourceType,
            Language = source.Language,
            SegmentCount = source.SegmentCount,
            ImportedAt = source.ImportedAt,
            TextLength = source.OriginalText.Length
        });
    }

    [HttpGet("{id:guid}/segments-count")]
    public async Task<ActionResult<CountResponse>> SegmentsCount(Guid id, CancellationToken cancellationToken)
    {
        var source = await _contentImportService.GetSourceAsync(id, cancellationToken);
        return source is null
            ? NotFound()
            : Ok(new CountResponse { Scope = "segments", Count = source.SegmentCount });
    }

    [HttpGet("{id:guid}/preview")]
    public async Task<ActionResult<TextPreviewResponse>> Preview(Guid id, [FromQuery] int length = 160, CancellationToken cancellationToken = default)
    {
        var source = await _contentImportService.GetSourceAsync(id, cancellationToken);
        if (source is null)
        {
            return NotFound();
        }

        var safeLength = Math.Clamp(length, 20, 1000);
        return Ok(new TextPreviewResponse
        {
            Id = source.Id,
            Preview = source.OriginalText.Length <= safeLength ? source.OriginalText : source.OriginalText[..safeLength],
            Length = source.OriginalText.Length
        });
    }

    [HttpGet("{id:guid}/exists")]
    public async Task<ActionResult<ExistsResponse>> Exists(Guid id, CancellationToken cancellationToken)
    {
        var source = await _contentImportService.GetSourceAsync(id, cancellationToken);
        return Ok(new ExistsResponse { Id = id, Exists = source is not null });
    }

    [HttpPost("preview-segments")]
    public ActionResult<SegmentPreviewResponse> PreviewSegments(TextSegmentationRequest request)
    {
        var segments = ContentImportService.SplitIntoSegments(request.Text);
        return Ok(new SegmentPreviewResponse
        {
            SegmentCount = segments.Count,
            Segments = segments.Take(request.MaxSegments).ToList()
        });
    }

    [HttpPost("validate")]
    public ActionResult<ValidationResponse> Validate(ContentSourceRequest request)
    {
        return Ok(new ValidationResponse { IsValid = ModelState.IsValid });
    }

    [HttpGet("types")]
    public ActionResult<IReadOnlyList<EnumOptionResponse>> Types()
    {
        return Ok(Enum.GetValues<ContentSourceType>()
            .Select(value => new EnumOptionResponse { Name = value.ToString(), Value = (int)value })
            .ToList());
    }
}
