using Microsoft.EntityFrameworkCore;
using VocabMinerCourseWork.Api.Data;
using VocabMinerCourseWork.Api.Domains.Entities;

namespace VocabMinerCourseWork.Api.Repositories;

public interface IExportRepository
{
    Task<IReadOnlyList<ExportJob>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ExportJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ExportJob> AddAsync(ExportJob job, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class ExportRepository : IExportRepository
{
    private readonly VocabMinerDbContext _dbContext;

    public ExportRepository(VocabMinerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ExportJob>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ExportJobs
            .AsNoTracking()
            .Where(job => job.UserId == userId)
            .OrderByDescending(job => job.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<ExportJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbContext.ExportJobs.FirstOrDefaultAsync(job => job.Id == id, cancellationToken);
    }

    public async Task<ExportJob> AddAsync(ExportJob job, CancellationToken cancellationToken = default)
    {
        _dbContext.ExportJobs.Add(job);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return job;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.ExportJobs.FindAsync(new object[] { id }, cancellationToken);
        if (job is null)
        {
            return false;
        }

        _dbContext.ExportJobs.Remove(job);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
