using System.Text.RegularExpressions;
using VocabMinerCourseWork.Api.Domains.Entities;
using VocabMinerCourseWork.Api.Domains.ViewModels;
using VocabMinerCourseWork.Api.Repositories;

namespace VocabMinerCourseWork.Api.BusinessLogic;

public interface IContentImportService
{
    Task<IReadOnlyList<ContentSourceResponse>> ListSourcesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ContentSourceResponse?> GetSourceAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ContentSourceResponse?> ImportAsync(ContentSourceRequest request, CancellationToken cancellationToken = default);

    Task<ContentSourceResponse?> UpdateSourceAsync(Guid id, ContentSourceUpdateRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteSourceAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SegmentResponse>> ListSegmentsAsync(Guid contentSourceId, CancellationToken cancellationToken = default);

    Task<SegmentResponse?> GetSegmentAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SegmentResponse?> UpdateSegmentAsync(Guid id, SegmentUpdateRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteSegmentAsync(Guid id, CancellationToken cancellationToken = default);
}

public class ContentImportService : IContentImportService
{
    private readonly IContentSourceRepository _contentSourceRepository;
    private readonly ISegmentRepository _segmentRepository;
    private readonly IUserRepository _userRepository;

    public ContentImportService(
        IContentSourceRepository contentSourceRepository,
        ISegmentRepository segmentRepository,
        IUserRepository userRepository)
    {
        _contentSourceRepository = contentSourceRepository;
        _segmentRepository = segmentRepository;
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<ContentSourceResponse>> ListSourcesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sources = await _contentSourceRepository.ListByUserAsync(userId, cancellationToken);
        return sources.Select(ToResponse).ToList();
    }

    public async Task<ContentSourceResponse?> GetSourceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var source = await _contentSourceRepository.GetByIdAsync(id, cancellationToken);
        return source is null ? null : ToResponse(source);
    }

    public async Task<ContentSourceResponse?> ImportAsync(ContentSourceRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var segments = SplitIntoSegments(request.OriginalText);
        var source = new ContentSource
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = request.Title.Trim(),
            SourceType = request.SourceType,
            OriginalText = request.OriginalText.Trim(),
            Language = request.Language.Trim().ToLowerInvariant(),
            ImportedAt = now,
            SegmentCount = segments.Count,
            Notes = request.Notes
        };

        for (var index = 0; index < segments.Count; index++)
        {
            source.Segments.Add(new Segment
            {
                Id = Guid.NewGuid(),
                ContentSourceId = source.Id,
                Position = index + 1,
                Text = segments[index],
                CreatedAt = now
            });
        }

        await _contentSourceRepository.AddAsync(source, cancellationToken);
        return ToResponse(source);
    }

    public async Task<ContentSourceResponse?> UpdateSourceAsync(Guid id, ContentSourceUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var source = await _contentSourceRepository.GetByIdAsync(id, cancellationToken);
        if (source is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var segments = SplitIntoSegments(request.OriginalText);
        source.Title = request.Title.Trim();
        source.SourceType = request.SourceType;
        source.OriginalText = request.OriginalText.Trim();
        source.Language = request.Language.Trim().ToLowerInvariant();
        source.Notes = request.Notes;
        source.SegmentCount = segments.Count;
        source.Segments.Clear();

        for (var index = 0; index < segments.Count; index++)
        {
            source.Segments.Add(new Segment
            {
                Id = Guid.NewGuid(),
                ContentSourceId = source.Id,
                Position = index + 1,
                Text = segments[index],
                CreatedAt = now
            });
        }

        await _contentSourceRepository.UpdateAsync(source, cancellationToken);
        return ToResponse(source);
    }

    public Task<bool> DeleteSourceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _contentSourceRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<SegmentResponse>> ListSegmentsAsync(Guid contentSourceId, CancellationToken cancellationToken = default)
    {
        var segments = await _segmentRepository.ListByContentSourceAsync(contentSourceId, cancellationToken);
        return segments.Select(ToResponse).ToList();
    }

    public async Task<SegmentResponse?> GetSegmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var segment = await _segmentRepository.GetByIdAsync(id, cancellationToken);
        return segment is null ? null : ToResponse(segment);
    }

    public async Task<SegmentResponse?> UpdateSegmentAsync(Guid id, SegmentUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var segment = await _segmentRepository.GetByIdAsync(id, cancellationToken);
        if (segment is null)
        {
            return null;
        }

        segment.Text = request.Text.Trim();
        await _segmentRepository.UpdateAsync(segment, cancellationToken);
        return ToResponse(segment);
    }

    public Task<bool> DeleteSegmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _segmentRepository.DeleteAsync(id, cancellationToken);
    }

    public static List<string> SplitIntoSegments(string originalText)
    {
        var normalized = Regex.Replace(originalText.Trim(), @"\r\n?", "\n");
        var lineSegments = normalized
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        if (lineSegments.Count > 1)
        {
            return lineSegments;
        }

        return Regex.Split(normalized, @"(?<=[.!?])\s+")
            .Select(segment => segment.Trim())
            .Where(segment => segment.Length > 0)
            .ToList();
    }

    private static ContentSourceResponse ToResponse(ContentSource source)
    {
        return new ContentSourceResponse
        {
            Id = source.Id,
            UserId = source.UserId,
            Title = source.Title,
            SourceType = source.SourceType,
            OriginalText = source.OriginalText,
            Language = source.Language,
            ImportedAt = source.ImportedAt,
            SegmentCount = source.SegmentCount,
            Notes = source.Notes
        };
    }

    private static SegmentResponse ToResponse(Segment segment)
    {
        return new SegmentResponse
        {
            Id = segment.Id,
            ContentSourceId = segment.ContentSourceId,
            Position = segment.Position,
            Text = segment.Text,
            StartTime = segment.StartTime,
            EndTime = segment.EndTime,
            CreatedAt = segment.CreatedAt
        };
    }
}
