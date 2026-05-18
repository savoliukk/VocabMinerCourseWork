using VocabMinerCourseWork.Api.BusinessLogic;
using VocabMinerCourseWork.Api.Domains.Entities;
using VocabMinerCourseWork.Api.Domains.ViewModels;
using VocabMinerCourseWork.Api.Repositories;

namespace VocabMinerCourseWork.Tests;

public class BusinessLogicTests
{
    [Fact]
    public void SplitIntoSegments_UsesNonEmptyLines_WhenTextHasLineBreaks()
    {
        var text = "First sentence.\n\nSecond sentence.\nThird sentence.";

        var segments = ContentImportService.SplitIntoSegments(text);

        Assert.Equal(3, segments.Count);
        Assert.Equal("First sentence.", segments[0]);
        Assert.Equal("Third sentence.", segments[2]);
    }

    [Fact]
    public void NormalizeTerm_TrimsLowercasesAndCollapsesSpaces()
    {
        var normalized = LearningUnitService.NormalizeTerm("  Real   Content  ");

        Assert.Equal("real content", normalized);
    }

    [Fact]
    public void CalculateNextDueAt_SchedulesEasyCardLaterThanHardCard()
    {
        var now = new DateTime(2026, 05, 03, 12, 0, 0, DateTimeKind.Utc);

        var hard = ReviewService.CalculateNextDueAt(now, ReviewGrade.Hard, 3);
        var easy = ReviewService.CalculateNextDueAt(now, ReviewGrade.Easy, 3);

        Assert.True(easy > hard);
    }

    [Fact]
    public void BuildDelimitedExport_CreatesTsvWithHeaderAndRows()
    {
        var units = new[]
        {
            new LearningUnit
            {
                Term = "vocabulary",
                Translation = "словниковий запас",
                Explanation = "A set of known words.",
                ExampleSentence = "Vocabulary grows from context.",
                Status = LearningStatus.Learning
            }
        };

        var payload = ExportService.BuildDelimitedExport(units, ExportFormat.Tsv);

        Assert.Contains("Term\tTranslation\tExplanation\tExample\tStatus", payload);
        Assert.Contains("vocabulary\tсловниковий запас", payload);
    }

    [Fact]
    public async Task ImportAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        var contentSourceRepository = new FakeContentSourceRepository();
        var service = new ContentImportService(
            contentSourceRepository,
            new FakeSegmentRepository(),
            new MissingUserRepository());

        var result = await service.ImportAsync(new ContentSourceRequest
        {
            UserId = Guid.NewGuid(),
            Title = "Plain text",
            SourceType = ContentSourceType.PlainText,
            OriginalText = "First sentence. Second sentence.",
            Language = "en"
        });

        Assert.Null(result);
        Assert.False(contentSourceRepository.AddCalled);
    }

    private sealed class MissingUserRepository : IUserRepository
    {
        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(null);
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeContentSourceRepository : IContentSourceRepository
    {
        public bool AddCalled { get; private set; }

        public Task<IReadOnlyList<ContentSource>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<ContentSource?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<ContentSource> AddAsync(ContentSource source, CancellationToken cancellationToken = default)
        {
            AddCalled = true;
            throw new NotSupportedException();
        }

        public Task<ContentSource> UpdateAsync(ContentSource source, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeSegmentRepository : ISegmentRepository
    {
        public Task<IReadOnlyList<Segment>> ListByContentSourceAsync(Guid contentSourceId, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Segment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<Segment> UpdateAsync(Segment segment, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }
}
