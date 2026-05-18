using Microsoft.EntityFrameworkCore;
using VocabMinerCourseWork.Api.Domains.Entities;

namespace VocabMinerCourseWork.Api.Data;

public class VocabMinerDbContext : DbContext
{
    public VocabMinerDbContext(DbContextOptions<VocabMinerDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<ContentSource> ContentSources => Set<ContentSource>();

    public DbSet<Segment> Segments => Set<Segment>();

    public DbSet<LearningUnit> LearningUnits => Set<LearningUnit>();

    public DbSet<Occurrence> Occurrences => Set<Occurrence>();

    public DbSet<ReviewAttempt> ReviewAttempts => Set<ReviewAttempt>();

    public DbSet<ExportJob> ExportJobs => Set<ExportJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureContentSources(modelBuilder);
        ConfigureSegments(modelBuilder);
        ConfigureLearningUnits(modelBuilder);
        ConfigureOccurrences(modelBuilder);
        ConfigureReviewAttempts(modelBuilder);
        ConfigureExportJobs(modelBuilder);
        SeedData(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Email).HasMaxLength(160).IsRequired();
            entity.Property(user => user.DisplayName).HasMaxLength(120).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(128).IsRequired();
            entity.Property(user => user.NativeLanguage).HasMaxLength(16).IsRequired();
            entity.Property(user => user.TargetLanguage).HasMaxLength(16).IsRequired();
        });
    }

    private static void ConfigureContentSources(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContentSource>(entity =>
        {
            entity.HasKey(source => source.Id);
            entity.HasIndex(source => new { source.UserId, source.Title });
            entity.Property(source => source.Title).HasMaxLength(200).IsRequired();
            entity.Property(source => source.SourceType).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(source => source.OriginalText).IsRequired();
            entity.Property(source => source.Language).HasMaxLength(16).IsRequired();
            entity.Property(source => source.Notes).HasMaxLength(500);

            entity.HasOne(source => source.User)
                .WithMany(user => user.ContentSources)
                .HasForeignKey(source => source.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureSegments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Segment>(entity =>
        {
            entity.HasKey(segment => segment.Id);
            entity.HasIndex(segment => new { segment.ContentSourceId, segment.Position }).IsUnique();
            entity.Property(segment => segment.Text).IsRequired();

            entity.HasOne(segment => segment.ContentSource)
                .WithMany(source => source.Segments)
                .HasForeignKey(segment => segment.ContentSourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureLearningUnits(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LearningUnit>(entity =>
        {
            entity.HasKey(unit => unit.Id);
            entity.HasIndex(unit => new { unit.UserId, unit.NormalizedTerm }).IsUnique();
            entity.Property(unit => unit.Term).HasMaxLength(200).IsRequired();
            entity.Property(unit => unit.NormalizedTerm).HasMaxLength(200).IsRequired();
            entity.Property(unit => unit.UnitType).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(unit => unit.Translation).HasMaxLength(300);
            entity.Property(unit => unit.Explanation).HasMaxLength(1200);
            entity.Property(unit => unit.ExampleSentence).HasMaxLength(1000);
            entity.Property(unit => unit.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

            entity.HasOne(unit => unit.User)
                .WithMany(user => user.LearningUnits)
                .HasForeignKey(unit => unit.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureOccurrences(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Occurrence>(entity =>
        {
            entity.HasKey(occurrence => occurrence.Id);
            entity.HasIndex(occurrence => new { occurrence.LearningUnitId, occurrence.SegmentId });
            entity.Property(occurrence => occurrence.ContextBefore).HasMaxLength(1000);
            entity.Property(occurrence => occurrence.ContextText).HasMaxLength(1000).IsRequired();
            entity.Property(occurrence => occurrence.ContextAfter).HasMaxLength(1000);

            entity.HasOne(occurrence => occurrence.LearningUnit)
                .WithMany(unit => unit.Occurrences)
                .HasForeignKey(occurrence => occurrence.LearningUnitId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(occurrence => occurrence.Segment)
                .WithMany(segment => segment.Occurrences)
                .HasForeignKey(occurrence => occurrence.SegmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(occurrence => occurrence.ContentSource)
                .WithMany(source => source.Occurrences)
                .HasForeignKey(occurrence => occurrence.ContentSourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureReviewAttempts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReviewAttempt>(entity =>
        {
            entity.HasKey(attempt => attempt.Id);
            entity.HasIndex(attempt => new { attempt.UserId, attempt.ReviewedAt });
            entity.Property(attempt => attempt.Grade).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(attempt => attempt.Notes).HasMaxLength(500);

            entity.HasOne(attempt => attempt.LearningUnit)
                .WithMany(unit => unit.ReviewAttempts)
                .HasForeignKey(attempt => attempt.LearningUnitId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(attempt => attempt.User)
                .WithMany(user => user.ReviewAttempts)
                .HasForeignKey(attempt => attempt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureExportJobs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExportJob>(entity =>
        {
            entity.HasKey(job => job.Id);
            entity.HasIndex(job => new { job.UserId, job.CreatedAt });
            entity.Property(job => job.Format).HasConversion<string>().HasMaxLength(16).IsRequired();
            entity.Property(job => job.FileName).HasMaxLength(240).IsRequired();
            entity.Property(job => job.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(job => job.FilterStatus).HasConversion<string>().HasMaxLength(32);
            entity.Property(job => job.Payload).IsRequired();

            entity.HasOne(job => job.User)
                .WithMany(user => user.ExportJobs)
                .HasForeignKey(job => job.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var sourceId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var segmentOneId = Guid.Parse("33333333-3333-3333-3333-333333333331");
        var segmentTwoId = Guid.Parse("33333333-3333-3333-3333-333333333332");
        var unitId = Guid.Parse("44444444-4444-4444-4444-444444444441");
        var occurrenceId = Guid.Parse("55555555-5555-5555-5555-555555555551");
        var reviewedUnitId = Guid.Parse("44444444-4444-4444-4444-444444444442");
        var reviewAttemptId = Guid.Parse("66666666-6666-6666-6666-666666666661");
        var exportJobId = Guid.Parse("77777777-7777-7777-7777-777777777771");
        var seedDate = new DateTime(2026, 05, 03, 12, 0, 0, DateTimeKind.Utc);

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = userId,
            Email = "student@example.com",
            DisplayName = "Demo Student",
            PasswordHash = "a109e36947ad56de1dca1cc49f0ef8ac9ad9a7b1aa0df41fb3c4cb73c1ff01ea",
            NativeLanguage = "uk",
            TargetLanguage = "en",
            CreatedAt = seedDate,
            LastLoginAt = null,
            IsActive = true
        });

        modelBuilder.Entity<ContentSource>().HasData(new ContentSource
        {
            Id = sourceId,
            UserId = userId,
            Title = "Demo English subtitles",
            SourceType = ContentSourceType.Subtitle,
            OriginalText = "Learning from real content helps vocabulary stick.\nA learner saves useful phrases with context.",
            Language = "en",
            ImportedAt = seedDate,
            SegmentCount = 2,
            Notes = "Seed content for coursework demonstration."
        });

        modelBuilder.Entity<Segment>().HasData(
            new Segment
            {
                Id = segmentOneId,
                ContentSourceId = sourceId,
                Position = 1,
                Text = "Learning from real content helps vocabulary stick.",
                StartTime = TimeSpan.Zero,
                EndTime = TimeSpan.FromSeconds(4),
                CreatedAt = seedDate
            },
            new Segment
            {
                Id = segmentTwoId,
                ContentSourceId = sourceId,
                Position = 2,
                Text = "A learner saves useful phrases with context.",
                StartTime = TimeSpan.FromSeconds(5),
                EndTime = TimeSpan.FromSeconds(9),
                CreatedAt = seedDate
            });

        modelBuilder.Entity<LearningUnit>().HasData(
            new LearningUnit
            {
                Id = unitId,
                UserId = userId,
                Term = "vocabulary",
                NormalizedTerm = "vocabulary",
                UnitType = LearningUnitType.Word,
                Translation = "словниковий запас",
                Explanation = "A set of words known or used by a person.",
                ExampleSentence = "Learning from real content helps vocabulary stick.",
                Status = LearningStatus.Learning,
                Difficulty = 2,
                ReviewDueAt = seedDate,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            },
            new LearningUnit
            {
                Id = reviewedUnitId,
                UserId = userId,
                Term = "real content",
                NormalizedTerm = "real content",
                UnitType = LearningUnitType.Phrase,
                Translation = "реальний контент",
                Explanation = "Texts, subtitles, videos, and examples created for real communication.",
                ExampleSentence = "Learning from real content helps vocabulary stick.",
                Status = LearningStatus.Known,
                Difficulty = 1,
                ReviewDueAt = seedDate.AddDays(2),
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            });

        modelBuilder.Entity<Occurrence>().HasData(new Occurrence
        {
            Id = occurrenceId,
            LearningUnitId = unitId,
            SegmentId = segmentOneId,
            ContentSourceId = sourceId,
            ContextBefore = "Learning from real content helps",
            ContextText = "vocabulary",
            ContextAfter = "stick.",
            CharacterStart = 33,
            CharacterEnd = 43,
            CreatedAt = seedDate
        });

        modelBuilder.Entity<ReviewAttempt>().HasData(new ReviewAttempt
        {
            Id = reviewAttemptId,
            LearningUnitId = reviewedUnitId,
            UserId = userId,
            ReviewedAt = seedDate.AddDays(-1),
            Grade = ReviewGrade.Good,
            ResponseTimeSeconds = 12,
            NextDueAt = seedDate.AddDays(2),
            Notes = "Seed review attempt."
        });

        modelBuilder.Entity<ExportJob>().HasData(new ExportJob
        {
            Id = exportJobId,
            UserId = userId,
            Format = ExportFormat.Tsv,
            FileName = "vocabminer-export-seed.tsv",
            Status = ExportStatus.Completed,
            FilterStatus = null,
            CreatedAt = seedDate,
            CompletedAt = seedDate,
            RowCount = 2,
            Payload = "Term\tTranslation\tExplanation\tExample\nvocabulary\tсловниковий запас\tA set of words known or used by a person.\tLearning from real content helps vocabulary stick."
        });
    }
}
