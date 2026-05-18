using Microsoft.AspNetCore.Mvc;
using VocabMinerCourseWork.Api.BusinessLogic;
using VocabMinerCourseWork.Api.Domains.ViewModels;

namespace VocabMinerCourseWork.Api.Controllers;

[ApiController]
[Route("segments")]
public class SegmentsController : ControllerBase
{
    private readonly IContentImportService _contentImportService;

    public SegmentsController(IContentImportService contentImportService)
    {
        _contentImportService = contentImportService;
    }

    [HttpGet("content-source/{contentSourceId:guid}")]
    public async Task<ActionResult<IReadOnlyList<SegmentResponse>>> ListByContentSource(Guid contentSourceId, CancellationToken cancellationToken)
    {
        return Ok(await _contentImportService.ListSegmentsAsync(contentSourceId, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SegmentResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var segment = await _contentImportService.GetSegmentAsync(id, cancellationToken);
        return segment is null ? NotFound() : Ok(segment);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SegmentResponse>> Update(Guid id, SegmentUpdateRequest request, CancellationToken cancellationToken)
    {
        var segment = await _contentImportService.UpdateSegmentAsync(id, request, cancellationToken);
        return segment is null ? NotFound() : Ok(segment);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _contentImportService.DeleteSegmentAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{id:guid}/exists")]
    public async Task<ActionResult<ExistsResponse>> Exists(Guid id, CancellationToken cancellationToken)
    {
        var segment = await _contentImportService.GetSegmentAsync(id, cancellationToken);
        return Ok(new ExistsResponse { Id = id, Exists = segment is not null });
    }

    [HttpGet("{id:guid}/length")]
    public async Task<ActionResult<SegmentLengthResponse>> Length(Guid id, CancellationToken cancellationToken)
    {
        var segment = await _contentImportService.GetSegmentAsync(id, cancellationToken);
        if (segment is null)
        {
            return NotFound();
        }

        return Ok(new SegmentLengthResponse
        {
            SegmentId = segment.Id,
            CharacterCount = segment.Text.Length,
            WordCount = segment.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length
        });
    }

    [HttpGet("{id:guid}/context")]
    public async Task<ActionResult<SegmentContextResponse>> Context(Guid id, CancellationToken cancellationToken)
    {
        var segment = await _contentImportService.GetSegmentAsync(id, cancellationToken);
        if (segment is null)
        {
            return NotFound();
        }

        var siblings = await _contentImportService.ListSegmentsAsync(segment.ContentSourceId, cancellationToken);
        var previous = siblings.LastOrDefault(item => item.Position < segment.Position);
        var next = siblings.FirstOrDefault(item => item.Position > segment.Position);
        return Ok(new SegmentContextResponse
        {
            SegmentId = segment.Id,
            Position = segment.Position,
            PreviousText = previous?.Text,
            CurrentText = segment.Text,
            NextText = next?.Text
        });
    }

    [HttpGet("content-source/{contentSourceId:guid}/count")]
    public async Task<ActionResult<CountResponse>> CountByContentSource(Guid contentSourceId, CancellationToken cancellationToken)
    {
        var segments = await _contentImportService.ListSegmentsAsync(contentSourceId, cancellationToken);
        return Ok(new CountResponse { Scope = "segments", Count = segments.Count });
    }

    [HttpGet("content-source/{contentSourceId:guid}/first")]
    public async Task<ActionResult<SegmentResponse>> First(Guid contentSourceId, CancellationToken cancellationToken)
    {
        var segments = await _contentImportService.ListSegmentsAsync(contentSourceId, cancellationToken);
        var segment = segments.FirstOrDefault();
        return segment is null ? NotFound() : Ok(segment);
    }

    [HttpGet("content-source/{contentSourceId:guid}/last")]
    public async Task<ActionResult<SegmentResponse>> Last(Guid contentSourceId, CancellationToken cancellationToken)
    {
        var segments = await _contentImportService.ListSegmentsAsync(contentSourceId, cancellationToken);
        var segment = segments.LastOrDefault();
        return segment is null ? NotFound() : Ok(segment);
    }

    [HttpGet("content-source/{contentSourceId:guid}/range")]
    public async Task<ActionResult<IReadOnlyList<SegmentResponse>>> Range(Guid contentSourceId, [FromQuery] int from = 1, [FromQuery] int to = 10, CancellationToken cancellationToken = default)
    {
        var lower = Math.Min(from, to);
        var upper = Math.Max(from, to);
        var segments = await _contentImportService.ListSegmentsAsync(contentSourceId, cancellationToken);
        return Ok(segments.Where(segment => segment.Position >= lower && segment.Position <= upper).ToList());
    }

    [HttpGet("content-source/{contentSourceId:guid}/search")]
    public async Task<ActionResult<IReadOnlyList<SegmentResponse>>> Search(Guid contentSourceId, [FromQuery] string term, CancellationToken cancellationToken)
    {
        var segments = await _contentImportService.ListSegmentsAsync(contentSourceId, cancellationToken);
        var safeTerm = term ?? string.Empty;
        return Ok(segments.Where(segment => segment.Text.Contains(safeTerm, StringComparison.OrdinalIgnoreCase)).ToList());
    }

    [HttpGet("content-source/{contentSourceId:guid}/positions")]
    public async Task<ActionResult<SegmentPositionsResponse>> Positions(Guid contentSourceId, CancellationToken cancellationToken)
    {
        var segments = await _contentImportService.ListSegmentsAsync(contentSourceId, cancellationToken);
        return Ok(new SegmentPositionsResponse
        {
            ContentSourceId = contentSourceId,
            Positions = segments.Select(segment => segment.Position).ToList()
        });
    }

    [HttpPost("split-preview")]
    public ActionResult<SegmentPreviewResponse> SplitPreview(TextSegmentationRequest request)
    {
        var segments = ContentImportService.SplitIntoSegments(request.Text);
        return Ok(new SegmentPreviewResponse
        {
            SegmentCount = segments.Count,
            Segments = segments.Take(request.MaxSegments).ToList()
        });
    }

    [HttpPost("normalize-preview")]
    public ActionResult<NormalizedTextResponse> NormalizePreview(SegmentUpdateRequest request)
    {
        var normalized = string.Join(' ', request.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return Ok(new NormalizedTextResponse
        {
            OriginalText = request.Text,
            NormalizedText = normalized
        });
    }

    [HttpGet("content-source/{contentSourceId:guid}/empty")]
    public async Task<ActionResult<IReadOnlyList<SegmentResponse>>> EmptySegments(Guid contentSourceId, CancellationToken cancellationToken)
    {
        var segments = await _contentImportService.ListSegmentsAsync(contentSourceId, cancellationToken);
        return Ok(segments.Where(segment => string.IsNullOrWhiteSpace(segment.Text)).ToList());
    }
}
