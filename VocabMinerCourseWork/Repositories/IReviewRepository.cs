using Microsoft.EntityFrameworkCore;
using VocabMinerCourseWork.Api.Data;
using VocabMinerCourseWork.Api.Domains.Entities;

namespace VocabMinerCourseWork.Api.Repositories;

public interface IReviewRepository
{
    Task<IReadOnlyList<LearningUnit>> GetDueUnitsAsync(Guid userId, DateTime now, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReviewAttempt>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ReviewAttempt> AddAttemptAsync(ReviewAttempt attempt, CancellationToken cancellationToken = default);
}

public class ReviewRepository : IReviewRepository
{
    private readonly VocabMinerDbContext _dbContext;

    public ReviewRepository(VocabMinerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<LearningUnit>> GetDueUnitsAsync(Guid userId, DateTime now, CancellationToken cancellationToken = default)
    {
        return await _dbContext.LearningUnits
            .AsNoTracking()
            .Where(unit => unit.UserId == userId &&
                           unit.Status != LearningStatus.Ignored &&
                           unit.ReviewDueAt <= now)
            .OrderBy(unit => unit.ReviewDueAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReviewAttempt>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ReviewAttempts
            .AsNoTracking()
            .Where(attempt => attempt.UserId == userId)
            .OrderByDescending(attempt => attempt.ReviewedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ReviewAttempt> AddAttemptAsync(ReviewAttempt attempt, CancellationToken cancellationToken = default)
    {
        _dbContext.ReviewAttempts.Add(attempt);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return attempt;
    }
}
