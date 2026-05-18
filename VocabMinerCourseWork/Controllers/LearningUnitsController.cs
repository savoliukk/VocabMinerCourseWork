using Microsoft.AspNetCore.Mvc;
using VocabMinerCourseWork.Api.BusinessLogic;
using VocabMinerCourseWork.Api.Domains.Entities;
using VocabMinerCourseWork.Api.Domains.ViewModels;

namespace VocabMinerCourseWork.Api.Controllers;

[ApiController]
[Route("learning-units")]
public class LearningUnitsController : ControllerBase
{
    private readonly ILearningUnitService _learningUnitService;

    public LearningUnitsController(ILearningUnitService learningUnitService)
    {
        _learningUnitService = learningUnitService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LearningUnitResponse>>> List(
        [FromQuery] Guid userId,
        [FromQuery] LearningStatus? status,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        return Ok(await _learningUnitService.ListAsync(userId, status, search, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LearningUnitResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var unit = await _learningUnitService.GetAsync(id, cancellationToken);
        return unit is null ? NotFound() : Ok(unit);
    }

    [HttpPost]
    public async Task<ActionResult<LearningUnitResponse>> Create(LearningUnitRequest request, CancellationToken cancellationToken)
    {
        var unit = await _learningUnitService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = unit.Id }, unit);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LearningUnitResponse>> Update(Guid id, LearningUnitUpdateRequest request, CancellationToken cancellationToken)
    {
        var unit = await _learningUnitService.UpdateAsync(id, request, cancellationToken);
        return unit is null ? NotFound() : Ok(unit);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _learningUnitService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/explain")]
    public async Task<ActionResult<ExplanationResponse>> Explain(Guid id, CancellationToken cancellationToken)
    {
        var explanation = await _learningUnitService.ExplainAsync(id, cancellationToken);
        return explanation is null ? NotFound() : Ok(explanation);
    }

    [HttpPost("promote-phrase")]
    public async Task<ActionResult<LearningUnitResponse>> PromotePhrase(PromotePhraseRequest request, CancellationToken cancellationToken)
    {
        var unit = await _learningUnitService.PromotePhraseAsync(request, cancellationToken);
        return unit is null ? NotFound() : CreatedAtAction(nameof(Get), new { id = unit.Id }, unit);
    }

    [HttpGet("count")]
    public async Task<ActionResult<CountResponse>> Count([FromQuery] Guid userId, [FromQuery] LearningStatus? status, CancellationToken cancellationToken)
    {
        var units = await _learningUnitService.ListAsync(userId, status, null, cancellationToken);
        return Ok(new CountResponse { Scope = "learning-units", Count = units.Count });
    }

    [HttpGet("due")]
    public async Task<ActionResult<IReadOnlyList<LearningUnitResponse>>> Due([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        var units = await _learningUnitService.ListAsync(userId, null, null, cancellationToken);
        return Ok(units.Where(unit => unit.Status != LearningStatus.Ignored && unit.ReviewDueAt <= DateTime.UtcNow).ToList());
    }

    [HttpGet("known")]
    public async Task<ActionResult<IReadOnlyList<LearningUnitResponse>>> Known([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await _learningUnitService.ListAsync(userId, LearningStatus.Known, null, cancellationToken));
    }

    [HttpGet("new")]
    public async Task<ActionResult<IReadOnlyList<LearningUnitResponse>>> New([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await _learningUnitService.ListAsync(userId, LearningStatus.New, null, cancellationToken));
    }

    [HttpGet("mastered")]
    public async Task<ActionResult<IReadOnlyList<LearningUnitResponse>>> Mastered([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await _learningUnitService.ListAsync(userId, LearningStatus.Mastered, null, cancellationToken));
    }

    [HttpGet("ignored")]
    public async Task<ActionResult<IReadOnlyList<LearningUnitResponse>>> Ignored([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await _learningUnitService.ListAsync(userId, LearningStatus.Ignored, null, cancellationToken));
    }

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<LearningUnitResponse>>> Search([FromQuery] Guid userId, [FromQuery] string term, CancellationToken cancellationToken)
    {
        return Ok(await _learningUnitService.ListAsync(userId, null, term, cancellationToken));
    }

    [HttpGet("{id:guid}/occurrences-count")]
    public async Task<ActionResult<CountResponse>> OccurrencesCount(Guid id, CancellationToken cancellationToken)
    {
        var unit = await _learningUnitService.GetAsync(id, cancellationToken);
        return unit is null
            ? NotFound()
            : Ok(new CountResponse { Scope = "occurrences", Count = unit.OccurrenceCount });
    }

    [HttpGet("{id:guid}/review-state")]
    public async Task<ActionResult<LearningUnitReviewStateResponse>> ReviewState(Guid id, CancellationToken cancellationToken)
    {
        var unit = await _learningUnitService.GetAsync(id, cancellationToken);
        if (unit is null)
        {
            return NotFound();
        }

        return Ok(new LearningUnitReviewStateResponse
        {
            LearningUnitId = unit.Id,
            Status = unit.Status,
            Difficulty = unit.Difficulty,
            ReviewDueAt = unit.ReviewDueAt,
            IsDue = unit.ReviewDueAt <= DateTime.UtcNow
        });
    }

    [HttpPost("normalize")]
    public ActionResult<LearningUnitNormalizeResponse> Normalize(LearningUnitNormalizeRequest request)
    {
        return Ok(new LearningUnitNormalizeResponse
        {
            Term = request.Term,
            NormalizedTerm = LearningUnitService.NormalizeTerm(request.Term)
        });
    }

    [HttpPost("{id:guid}/mark-known")]
    public async Task<ActionResult<LearningUnitResponse>> MarkKnown(Guid id, CancellationToken cancellationToken)
    {
        var unit = await _learningUnitService.MarkStatusAsync(id, LearningStatus.Known, cancellationToken);
        return unit is null ? NotFound() : Ok(unit);
    }

    [HttpPost("{id:guid}/mark-ignored")]
    public async Task<ActionResult<LearningUnitResponse>> MarkIgnored(Guid id, CancellationToken cancellationToken)
    {
        var unit = await _learningUnitService.MarkStatusAsync(id, LearningStatus.Ignored, cancellationToken);
        return unit is null ? NotFound() : Ok(unit);
    }

    [HttpPost("{id:guid}/mark-learning")]
    public async Task<ActionResult<LearningUnitResponse>> MarkLearning(Guid id, CancellationToken cancellationToken)
    {
        var unit = await _learningUnitService.MarkStatusAsync(id, LearningStatus.Learning, cancellationToken);
        return unit is null ? NotFound() : Ok(unit);
    }
}
