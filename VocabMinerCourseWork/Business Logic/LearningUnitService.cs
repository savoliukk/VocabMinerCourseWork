using System.Globalization;
using VocabMinerCourseWork.Api.Domains.Entities;
using VocabMinerCourseWork.Api.Domains.ViewModels;
using VocabMinerCourseWork.Api.Repositories;

namespace VocabMinerCourseWork.Api.BusinessLogic;

public interface ILearningUnitService
{
    Task<IReadOnlyList<LearningUnitResponse>> ListAsync(Guid userId, LearningStatus? status, string? search, CancellationToken cancellationToken = default);

    Task<LearningUnitResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<LearningUnitResponse> CreateAsync(LearningUnitRequest request, CancellationToken cancellationToken = default);

    Task<LearningUnitResponse?> UpdateAsync(Guid id, LearningUnitUpdateRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ExplanationResponse?> ExplainAsync(Guid id, CancellationToken cancellationToken = default);

    Task<LearningUnitResponse?> PromotePhraseAsync(PromotePhraseRequest request, CancellationToken cancellationToken = default);

    Task<LearningUnitResponse?> MarkStatusAsync(Guid id, LearningStatus status, CancellationToken cancellationToken = default);
}

public class LearningUnitService : ILearningUnitService
{
    private readonly ILearningUnitRepository _learningUnitRepository;
    private readonly ISegmentRepository _segmentRepository;
    private readonly IMockExplanationService _mockExplanationService;

    public LearningUnitService(
        ILearningUnitRepository learningUnitRepository,
        ISegmentRepository segmentRepository,
        IMockExplanationService mockExplanationService)
    {
        _learningUnitRepository = learningUnitRepository;
        _segmentRepository = segmentRepository;
        _mockExplanationService = mockExplanationService;
    }

    public async Task<IReadOnlyList<LearningUnitResponse>> ListAsync(Guid userId, LearningStatus? status, string? search, CancellationToken cancellationToken = default)
    {
        var units = await _learningUnitRepository.ListAsync(userId, status, search, cancellationToken);
        return units.Select(ToResponse).ToList();
    }

    public async Task<LearningUnitResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var unit = await _learningUnitRepository.GetByIdAsync(id, cancellationToken);
        return unit is null ? null : ToResponse(unit);
    }

    public async Task<LearningUnitResponse> CreateAsync(LearningUnitRequest request, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeTerm(request.Term);
        var existing = await _learningUnitRepository.GetByNormalizedTermAsync(request.UserId, normalized, cancellationToken);
        if (existing is not null)
        {
            if (await AttachOccurrenceIfNeeded(existing, request.SegmentId, cancellationToken))
            {
                existing.UpdatedAt = DateTime.UtcNow;
                await _learningUnitRepository.UpdateAsync(existing, cancellationToken);
            }

            return ToResponse(existing);
        }

        var now = DateTime.UtcNow;
        var unit = new LearningUnit
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Term = request.Term.Trim(),
            NormalizedTerm = normalized,
            UnitType = request.UnitType,
            Translation = request.Translation,
            Explanation = request.Explanation,
            Status = LearningStatus.New,
            Difficulty = 1,
            ReviewDueAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        await AttachOccurrenceIfNeeded(unit, request.SegmentId, cancellationToken);
        await _learningUnitRepository.AddAsync(unit, cancellationToken);
        return ToResponse(unit);
    }

    public async Task<LearningUnitResponse?> UpdateAsync(Guid id, LearningUnitUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var unit = await _learningUnitRepository.GetByIdAsync(id, cancellationToken);
        if (unit is null)
        {
            return null;
        }

        unit.Term = request.Term.Trim();
        unit.NormalizedTerm = NormalizeTerm(request.Term);
        unit.UnitType = request.UnitType;
        unit.Translation = request.Translation;
        unit.Explanation = request.Explanation;
        unit.ExampleSentence = request.ExampleSentence;
        unit.Status = request.Status;
        unit.Difficulty = request.Difficulty;
        unit.UpdatedAt = DateTime.UtcNow;
        await _learningUnitRepository.UpdateAsync(unit, cancellationToken);
        return ToResponse(unit);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _learningUnitRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task<ExplanationResponse?> ExplainAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var unit = await _learningUnitRepository.GetByIdAsync(id, cancellationToken);
        if (unit is null)
        {
            return null;
        }

        var context = unit.ExampleSentence ?? unit.Occurrences.FirstOrDefault()?.Segment?.Text;
        var explanation = _mockExplanationService.Generate(unit.Id, unit.Term, context);
        unit.Translation = explanation.Translation;
        unit.Explanation = explanation.Explanation;
        unit.ExampleSentence = explanation.ExampleSentence;
        unit.UpdatedAt = DateTime.UtcNow;
        await _learningUnitRepository.UpdateAsync(unit, cancellationToken);
        return explanation;
    }

    public async Task<LearningUnitResponse?> PromotePhraseAsync(PromotePhraseRequest request, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeTerm(request.Phrase);
        var existing = await _learningUnitRepository.GetByNormalizedTermAsync(request.UserId, normalized, cancellationToken);
        if (existing is not null)
        {
            if (await AttachOccurrenceIfNeeded(existing, request.SegmentId, cancellationToken))
            {
                existing.UpdatedAt = DateTime.UtcNow;
                await _learningUnitRepository.UpdateAsync(existing, cancellationToken);
            }

            return ToResponse(existing);
        }

        return await CreateAsync(new LearningUnitRequest
        {
            UserId = request.UserId,
            Term = request.Phrase,
            UnitType = LearningUnitType.Phrase,
            SegmentId = request.SegmentId
        }, cancellationToken);
    }

    public async Task<LearningUnitResponse?> MarkStatusAsync(Guid id, LearningStatus status, CancellationToken cancellationToken = default)
    {
        var unit = await _learningUnitRepository.GetByIdAsync(id, cancellationToken);
        if (unit is null)
        {
            return null;
        }

        unit.Status = status;
        unit.UpdatedAt = DateTime.UtcNow;
        if (status == LearningStatus.New || status == LearningStatus.Learning)
        {
            unit.ReviewDueAt = DateTime.UtcNow;
        }

        await _learningUnitRepository.UpdateAsync(unit, cancellationToken);
        return ToResponse(unit);
    }

    public static string NormalizeTerm(string term)
    {
        return string.Join(' ', term.Trim().ToLower(CultureInfo.InvariantCulture).Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private async Task<bool> AttachOccurrenceIfNeeded(LearningUnit unit, Guid? segmentId, CancellationToken cancellationToken)
    {
        if (!segmentId.HasValue || unit.Occurrences.Any(occurrence => occurrence.SegmentId == segmentId.Value))
        {
            return false;
        }

        var segment = await _segmentRepository.GetByIdAsync(segmentId.Value, cancellationToken);
        if (segment is null)
        {
            return false;
        }

        var text = segment.Text;
        var index = text.IndexOf(unit.Term, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            index = 0;
        }

        unit.ExampleSentence ??= text;
        unit.Occurrences.Add(new Occurrence
        {
            Id = Guid.NewGuid(),
            LearningUnitId = unit.Id,
            SegmentId = segment.Id,
            ContentSourceId = segment.ContentSourceId,
            ContextBefore = text[..Math.Min(index, text.Length)],
            ContextText = unit.Term,
            ContextAfter = index < text.Length ? text[(Math.Min(index + unit.Term.Length, text.Length))..] : string.Empty,
            CharacterStart = index,
            CharacterEnd = Math.Min(index + unit.Term.Length, text.Length),
            CreatedAt = DateTime.UtcNow
        });

        return true;
    }

    private static LearningUnitResponse ToResponse(LearningUnit unit)
    {
        return new LearningUnitResponse
        {
            Id = unit.Id,
            UserId = unit.UserId,
            Term = unit.Term,
            NormalizedTerm = unit.NormalizedTerm,
            UnitType = unit.UnitType,
            Translation = unit.Translation,
            Explanation = unit.Explanation,
            ExampleSentence = unit.ExampleSentence,
            Status = unit.Status,
            Difficulty = unit.Difficulty,
            ReviewDueAt = unit.ReviewDueAt,
            CreatedAt = unit.CreatedAt,
            UpdatedAt = unit.UpdatedAt,
            OccurrenceCount = unit.Occurrences.Count
        };
    }
}
