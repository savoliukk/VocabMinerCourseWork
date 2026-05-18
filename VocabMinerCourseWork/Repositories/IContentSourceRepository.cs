using Microsoft.EntityFrameworkCore;
using VocabMinerCourseWork.Api.Data;
using VocabMinerCourseWork.Api.Domains.Entities;

namespace VocabMinerCourseWork.Api.Repositories;

public interface IContentSourceRepository
{
    Task<IReadOnlyList<ContentSource>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ContentSource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ContentSource> AddAsync(ContentSource source, CancellationToken cancellationToken = default);

    Task<ContentSource> UpdateAsync(ContentSource source, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class ContentSourceRepository : IContentSourceRepository
{
    private readonly VocabMinerDbContext _dbContext;

    public ContentSourceRepository(VocabMinerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ContentSource>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ContentSources
            .AsNoTracking()
            .Where(source => source.UserId == userId)
            .OrderByDescending(source => source.ImportedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<ContentSource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.ContentSources
            .Include(source => source.Segments.OrderBy(segment => segment.Position))
            .FirstOrDefaultAsync(source => source.Id == id, cancellationToken);
    }

    public async Task<ContentSource> AddAsync(ContentSource source, CancellationToken cancellationToken = default)
    {
        _dbContext.ContentSources.Add(source);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return source;
    }

    public async Task<ContentSource> UpdateAsync(ContentSource source, CancellationToken cancellationToken = default)
    {
        _dbContext.ContentSources.Update(source);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return source;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var source = await _dbContext.ContentSources.FindAsync(new object[] { id }, cancellationToken);
        if (source is null)
        {
            return false;
        }

        _dbContext.ContentSources.Remove(source);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
