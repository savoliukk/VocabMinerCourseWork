using Microsoft.AspNetCore.Mvc;
using VocabMinerCourseWork.Api.BusinessLogic;
using VocabMinerCourseWork.Api.Domains.Entities;
using VocabMinerCourseWork.Api.Domains.ViewModels;

namespace VocabMinerCourseWork.Api.Controllers;

[ApiController]
[Route("reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ILearningUnitService _learningUnitService;

    public ReviewsController(IReviewService reviewService, ILearningUnitService learningUnitService)
    {
        _reviewService = reviewService;
        _learningUnitService = learningUnitService;
    }

    [HttpGet("today/{userId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ReviewResponse>>> Today(Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await _reviewService.GetTodayAsync(userId, cancellationToken));
    }

    [HttpGet("history/{userId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ReviewAttemptResponse>>> History(Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await _reviewService.GetHistoryAsync(userId, cancellationToken));
    }

    [HttpPost("submit")]
    public async Task<ActionResult<ReviewResponse>> Submit(ReviewSubmitRequest request, CancellationToken cancellationToken)
    {
        var response = await _reviewService.SubmitAsync(request, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost("reset/{learningUnitId:guid}")]
    public async Task<ActionResult<ReviewResponse>> Reset(Guid learningUnitId, CancellationToken cancellationToken)
    {
        var response = await _reviewService.ResetAsync(learningUnitId, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpGet("queue/{userId:guid}")]
    public async Task<ActionResult<IReadOnlyList<ReviewResponse>>> Queue(Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await _reviewService.GetTodayAsync(userId, cancellationToken));
    }

    [HttpGet("count-due/{userId:guid}")]
    public async Task<ActionResult<CountResponse>> CountDue(Guid userId, CancellationToken cancellationToken)
    {
        var due = await _reviewService.GetTodayAsync(userId, cancellationToken);
        return Ok(new CountResponse { Scope = "reviews-due", Count = due.Count });
    }

    [HttpGet("history/{userId:guid}/latest")]
    public async Task<ActionResult<IReadOnlyList<ReviewAttemptResponse>>> LatestHistory(Guid userId, [FromQuery] int take = 5, CancellationToken cancellationToken = default)
    {
        var history = await _reviewService.GetHistoryAsync(userId, cancellationToken);
        return Ok(history.Take(Math.Clamp(take, 1, 50)).ToList());
    }

    [HttpGet("history/{userId:guid}/count")]
    public async Task<ActionResult<CountResponse>> HistoryCount(Guid userId, CancellationToken cancellationToken)
    {
        var history = await _reviewService.GetHistoryAsync(userId, cancellationToken);
        return Ok(new CountResponse { Scope = "review-history", Count = history.Count });
    }

    [HttpGet("stats/{userId:guid}")]
    public async Task<ActionResult<ReviewStatsResponse>> Stats(Guid userId, CancellationToken cancellationToken)
    {
        var due = await _reviewService.GetTodayAsync(userId, cancellationToken);
        var history = await _reviewService.GetHistoryAsync(userId, cancellationToken);
        return Ok(new ReviewStatsResponse
        {
            UserId = userId,
            DueCount = due.Count,
            HistoryCount = history.Count,
            AgainCount = history.Count(attempt => attempt.Grade == ReviewGrade.Again),
            HardCount = history.Count(attempt => attempt.Grade == ReviewGrade.Hard),
            GoodCount = history.Count(attempt => attempt.Grade == ReviewGrade.Good),
            EasyCount = history.Count(attempt => attempt.Grade == ReviewGrade.Easy)
        });
    }

    [HttpGet("grades/options")]
    public ActionResult<IReadOnlyList<EnumOptionResponse>> GradeOptions()
    {
        return Ok(Enum.GetValues<ReviewGrade>()
            .Select(value => new EnumOptionResponse { Name = value.ToString(), Value = (int)value })
            .ToList());
    }

    [HttpGet("next/{learningUnitId:guid}")]
    public async Task<ActionResult<LearningUnitReviewStateResponse>> Next(Guid learningUnitId, CancellationToken cancellationToken)
    {
        return await BuildReviewState(learningUnitId, cancellationToken);
    }

    [HttpGet("{learningUnitId:guid}/status")]
    public async Task<ActionResult<LearningUnitReviewStateResponse>> Status(Guid learningUnitId, CancellationToken cancellationToken)
    {
        return await BuildReviewState(learningUnitId, cancellationToken);
    }

    [HttpPost("submit-again")]
    public Task<ActionResult<ReviewResponse>> SubmitAgain(ReviewQuickSubmitRequest request, CancellationToken cancellationToken)
    {
        return SubmitQuick(request, ReviewGrade.Again, cancellationToken);
    }

    [HttpPost("submit-hard")]
    public Task<ActionResult<ReviewResponse>> SubmitHard(ReviewQuickSubmitRequest request, CancellationToken cancellationToken)
    {
        return SubmitQuick(request, ReviewGrade.Hard, cancellationToken);
    }

    [HttpPost("submit-good")]
    public Task<ActionResult<ReviewResponse>> SubmitGood(ReviewQuickSubmitRequest request, CancellationToken cancellationToken)
    {
        return SubmitQuick(request, ReviewGrade.Good, cancellationToken);
    }

    [HttpPost("submit-easy")]
    public Task<ActionResult<ReviewResponse>> SubmitEasy(ReviewQuickSubmitRequest request, CancellationToken cancellationToken)
    {
        return SubmitQuick(request, ReviewGrade.Easy, cancellationToken);
    }

    [HttpPost("reset-today/{userId:guid}")]
    public async Task<ActionResult<CountResponse>> ResetToday(Guid userId, CancellationToken cancellationToken)
    {
        var due = await _reviewService.GetTodayAsync(userId, cancellationToken);
        var count = 0;
        foreach (var item in due)
        {
            var response = await _reviewService.ResetAsync(item.LearningUnitId, cancellationToken);
            if (response is not null)
            {
                count++;
            }
        }

        return Ok(new CountResponse { Scope = "reviews-reset", Count = count });
    }

    private async Task<ActionResult<LearningUnitReviewStateResponse>> BuildReviewState(Guid learningUnitId, CancellationToken cancellationToken)
    {
        var unit = await _learningUnitService.GetAsync(learningUnitId, cancellationToken);
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

    private async Task<ActionResult<ReviewResponse>> SubmitQuick(ReviewQuickSubmitRequest request, ReviewGrade grade, CancellationToken cancellationToken)
    {
        var response = await _reviewService.SubmitAsync(new ReviewSubmitRequest
        {
            UserId = request.UserId,
            LearningUnitId = request.LearningUnitId,
            Grade = grade,
            ResponseTimeSeconds = request.ResponseTimeSeconds,
            Notes = request.Notes
        }, cancellationToken);

        return response is null ? NotFound() : Ok(response);
    }
}
