using VocabMinerCourseWork.Api.Domains.Entities;
using VocabMinerCourseWork.Api.Domains.ViewModels;
using VocabMinerCourseWork.Api.Repositories;

namespace VocabMinerCourseWork.Api.BusinessLogic;

public interface IReviewService
{
    Task<IReadOnlyList<ReviewResponse>> GetTodayAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReviewAttemptResponse>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ReviewResponse?> SubmitAsync(ReviewSubmitRequest request, CancellationToken cancellationToken = default);

    Task<ReviewResponse?> ResetAsync(Guid learningUnitId, CancellationToken cancellationToken = default);
}

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ILearningUnitRepository _learningUnitRepository;

    public ReviewService(IReviewRepository reviewRepository, ILearningUnitRepository learningUnitRepository)
    {
        _reviewRepository = reviewRepository;
        _learningUnitRepository = learningUnitRepository;
    }

    public async Task<IReadOnlyList<ReviewResponse>> GetTodayAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var dueUnits = await _reviewRepository.GetDueUnitsAsync(userId, DateTime.UtcNow, cancellationToken);
        return dueUnits.Select(ToReviewResponse).ToList();
    }

    public async Task<IReadOnlyList<ReviewAttemptResponse>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var attempts = await _reviewRepository.GetHistoryAsync(userId, cancellationToken);
        return attempts.Select(ToAttemptResponse).ToList();
    }

    public async Task<ReviewResponse?> SubmitAsync(ReviewSubmitRequest request, CancellationToken cancellationToken = default)
    {
        var unit = await _learningUnitRepository.GetByIdAsync(request.LearningUnitId, cancellationToken);
        if (unit is null || unit.UserId != request.UserId)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var nextDueAt = CalculateNextDueAt(now, request.Grade, unit.Difficulty);
        unit.ReviewDueAt = nextDueAt;
        unit.UpdatedAt = now;
        unit.Status = request.Grade switch
        {
            ReviewGrade.Again => LearningStatus.Learning,
            ReviewGrade.Easy => LearningStatus.Mastered,
            _ => LearningStatus.Known
        };
        unit.Difficulty = request.Grade switch
        {
            ReviewGrade.Again => Math.Min(5, unit.Difficulty + 1),
            ReviewGrade.Easy => Math.Max(1, unit.Difficulty - 1),
            _ => unit.Difficulty
        };

        await _learningUnitRepository.UpdateAsync(unit, cancellationToken);
        await _reviewRepository.AddAttemptAsync(new ReviewAttempt
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            LearningUnitId = request.LearningUnitId,
            Grade = request.Grade,
            ResponseTimeSeconds = request.ResponseTimeSeconds,
            ReviewedAt = now,
            NextDueAt = nextDueAt,
            Notes = request.Notes
        }, cancellationToken);

        return ToReviewResponse(unit);
    }

    public async Task<ReviewResponse?> ResetAsync(Guid learningUnitId, CancellationToken cancellationToken = default)
    {
        var unit = await _learningUnitRepository.GetByIdAsync(learningUnitId, cancellationToken);
        if (unit is null)
        {
            return null;
        }

        unit.Status = LearningStatus.New;
        unit.Difficulty = 1;
        unit.ReviewDueAt = DateTime.UtcNow;
        unit.UpdatedAt = DateTime.UtcNow;
        await _learningUnitRepository.UpdateAsync(unit, cancellationToken);
        return ToReviewResponse(unit);
    }

    public static DateTime CalculateNextDueAt(DateTime reviewedAt, ReviewGrade grade, int difficulty)
    {
        var days = grade switch
        {
            ReviewGrade.Again => 0,
            ReviewGrade.Hard => 1,
            ReviewGrade.Good => Math.Max(2, 4 - difficulty),
            ReviewGrade.Easy => Math.Max(4, 7 - difficulty),
            _ => 1
        };

        return reviewedAt.AddDays(days);
    }

    private static ReviewResponse ToReviewResponse(LearningUnit unit)
    {
        return new ReviewResponse
        {
            LearningUnitId = unit.Id,
            Term = unit.Term,
            Translation = unit.Translation,
            Explanation = unit.Explanation,
            ExampleSentence = unit.ExampleSentence,
            Status = unit.Status,
            ReviewDueAt = unit.ReviewDueAt
        };
    }

    private static ReviewAttemptResponse ToAttemptResponse(ReviewAttempt attempt)
    {
        return new ReviewAttemptResponse
        {
            Id = attempt.Id,
            LearningUnitId = attempt.LearningUnitId,
            Grade = attempt.Grade,
            ResponseTimeSeconds = attempt.ResponseTimeSeconds,
            ReviewedAt = attempt.ReviewedAt,
            NextDueAt = attempt.NextDueAt,
            Notes = attempt.Notes
        };
    }
}
