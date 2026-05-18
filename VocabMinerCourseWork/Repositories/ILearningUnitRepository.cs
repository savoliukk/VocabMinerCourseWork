using Microsoft.EntityFrameworkCore;
using VocabMinerCourseWork.Api.Data;
using VocabMinerCourseWork.Api.Domains.Entities;

namespace VocabMinerCourseWork.Api.Repositories;

public interface ILearningUnitRepository
{
    Task<IReadOnlyList<LearningUnit>> ListAsync(Guid userId, LearningStatus? status, string? search, CancellationToken cancellationToken = default);

    Task<LearningUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<LearningUnit?> GetByNormalizedTermAsync(Guid userId, string normalizedTerm, CancellationToken cancellationToken = default);

    Task<LearningUnit> AddAsync(LearningUnit unit, CancellationToken cancellationToken = default);

    Task<LearningUnit> UpdateAsync(LearningUnit unit, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class LearningUnitRepository : ILearningUnitRepository
{
    private readonly VocabMinerDbContext _dbContext;

    public LearningUnitRepository(VocabMinerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<LearningUnit>> ListAsync(Guid userId, LearningStatus? status, string? search, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.LearningUnits
            .AsNoTracking()
            .Include(unit => unit.Occurrences)
            .Where(unit => unit.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(unit => unit.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLowerInvariant();
            query = query.Where(unit => unit.NormalizedTerm.Contains(normalizedSearch));
        }

        return await query
            .OrderBy(unit => unit.ReviewDueAt)
            .ThenBy(unit => unit.Term)
            .ToListAsync(cancellationToken);
    }

    public Task<LearningUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.LearningUnits
            .Include(unit => unit.Occurrences)
            .ThenInclude(occurrence => occurrence.Segment)
            .FirstOrDefaultAsync(unit => unit.Id == id, cancellationToken);
    }

    public Task<LearningUnit?> GetByNormalizedTermAsync(Guid userId, string normalizedTerm, CancellationToken cancellationToken = default)
    {
        return _dbContext.LearningUnits
            .Include(unit => unit.Occurrences)
            .FirstOrDefaultAsync(unit => unit.UserId == userId && unit.NormalizedTerm == normalizedTerm, cancellationToken);
    }

    public async Task<LearningUnit> AddAsync(LearningUnit unit, CancellationToken cancellationToken = default)
    {
        _dbContext.LearningUnits.Add(unit);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return unit;
    }

    public async Task<LearningUnit> UpdateAsync(LearningUnit unit, CancellationToken cancellationToken = default)
    {
        _dbContext.LearningUnits.Update(unit);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return unit;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var unit = await _dbContext.LearningUnits.FindAsync(new object[] { id }, cancellationToken);
        if (unit is null)
        {
            return false;
        }

        _dbContext.LearningUnits.Remove(unit);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
