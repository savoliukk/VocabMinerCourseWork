using Microsoft.EntityFrameworkCore;
using VocabMinerCourseWork.Api.Data;
using VocabMinerCourseWork.Api.Domains.Entities;

namespace VocabMinerCourseWork.Api.Repositories;

public interface ISegmentRepository
{
    Task<IReadOnlyList<Segment>> ListByContentSourceAsync(Guid contentSourceId, CancellationToken cancellationToken = default);

    Task<Segment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Segment> UpdateAsync(Segment segment, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class SegmentRepository : ISegmentRepository
{
    private readonly VocabMinerDbContext _dbContext;

    public SegmentRepository(VocabMinerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Segment>> ListByContentSourceAsync(Guid contentSourceId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Segments
            .AsNoTracking()
            .Where(segment => segment.ContentSourceId == contentSourceId)
            .OrderBy(segment => segment.Position)
            .ToListAsync(cancellationToken);
    }

    public Task<Segment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Segments
            .Include(segment => segment.ContentSource)
            .FirstOrDefaultAsync(segment => segment.Id == id, cancellationToken);
    }

    public async Task<Segment> UpdateAsync(Segment segment, CancellationToken cancellationToken = default)
    {
        _dbContext.Segments.Update(segment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return segment;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var segment = await _dbContext.Segments.FindAsync(new object[] { id }, cancellationToken);
        if (segment is null)
        {
            return false;
        }

        _dbContext.Segments.Remove(segment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
