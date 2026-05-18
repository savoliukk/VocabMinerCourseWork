using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using VocabMinerCourseWork.Api.Data;

#nullable disable

namespace VocabMinerCourseWork.Api.Migrations;

[DbContext(typeof(VocabMinerDbContext))]
[Migration("20260503183000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                DisplayName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                PasswordHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                NativeLanguage = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                TargetLanguage = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ContentSources",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                SourceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                OriginalText = table.Column<string>(type: "text", nullable: false),
                Language = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                ImportedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                SegmentCount = table.Column<int>(type: "integer", nullable: false),
                Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContentSources", x => x.Id);
                table.ForeignKey(
                    name: "FK_ContentSources_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ExportJobs",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Format = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                FileName = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                FilterStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                RowCount = table.Column<int>(type: "integer", nullable: false),
                Payload = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExportJobs", x => x.Id);
                table.ForeignKey(
                    name: "FK_ExportJobs_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "LearningUnits",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Term = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                NormalizedTerm = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                UnitType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                Translation = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                Explanation = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: true),
                ExampleSentence = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                Difficulty = table.Column<int>(type: "integer", nullable: false),
                ReviewDueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LearningUnits", x => x.Id);
                table.ForeignKey(
                    name: "FK_LearningUnits_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Segments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                ContentSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                Position = table.Column<int>(type: "integer", nullable: false),
                Text = table.Column<string>(type: "text", nullable: false),
                StartTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                EndTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Segments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Segments_ContentSources_ContentSourceId",
                    column: x => x.ContentSourceId,
                    principalTable: "ContentSources",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "ReviewAttempts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                LearningUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Grade = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                ResponseTimeSeconds = table.Column<int>(type: "integer", nullable: false),
                NextDueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ReviewAttempts", x => x.Id);
                table.ForeignKey(
                    name: "FK_ReviewAttempts_LearningUnits_LearningUnitId",
                    column: x => x.LearningUnitId,
                    principalTable: "LearningUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ReviewAttempts_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Occurrences",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                LearningUnitId = table.Column<Guid>(type: "uuid", nullable: false),
                SegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                ContentSourceId = table.Column<Guid>(type: "uuid", nullable: false),
                ContextBefore = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                ContextText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                ContextAfter = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                CharacterStart = table.Column<int>(type: "integer", nullable: false),
                CharacterEnd = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Occurrences", x => x.Id);
                table.ForeignKey(
                    name: "FK_Occurrences_ContentSources_ContentSourceId",
                    column: x => x.ContentSourceId,
                    principalTable: "ContentSources",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Occurrences_LearningUnits_LearningUnitId",
                    column: x => x.LearningUnitId,
                    principalTable: "LearningUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Occurrences_Segments_SegmentId",
                    column: x => x.SegmentId,
                    principalTable: "Segments",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        var seedDate = new DateTime(2026, 05, 03, 12, 0, 0, DateTimeKind.Utc);
        migrationBuilder.InsertData(
            table: "Users",
            columns: new[] { "Id", "CreatedAt", "DisplayName", "Email", "IsActive", "LastLoginAt", "NativeLanguage", "PasswordHash", "TargetLanguage" },
            columnTypes: new[] { "uuid", "timestamp with time zone", "character varying(120)", "character varying(160)", "boolean", "timestamp with time zone", "character varying(16)", "character varying(128)", "character varying(16)" },
            values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), seedDate, "Demo Student", "student@example.com", true, null, "uk", "a109e36947ad56de1dca1cc49f0ef8ac9ad9a7b1aa0df41fb3c4cb73c1ff01ea", "en" });

        migrationBuilder.InsertData(
            table: "ContentSources",
            columns: new[] { "Id", "ImportedAt", "Language", "Notes", "OriginalText", "SegmentCount", "SourceType", "Title", "UserId" },
            columnTypes: new[] { "uuid", "timestamp with time zone", "character varying(16)", "character varying(500)", "text", "integer", "character varying(32)", "character varying(200)", "uuid" },
            values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), seedDate, "en", "Seed content for coursework demonstration.", "Learning from real content helps vocabulary stick.\nA learner saves useful phrases with context.", 2, "Subtitle", "Demo English subtitles", new Guid("11111111-1111-1111-1111-111111111111") });

        migrationBuilder.InsertData(
            table: "LearningUnits",
            columns: new[] { "Id", "CreatedAt", "Difficulty", "ExampleSentence", "Explanation", "NormalizedTerm", "ReviewDueAt", "Status", "Term", "Translation", "UnitType", "UpdatedAt", "UserId" },
            columnTypes: new[] { "uuid", "timestamp with time zone", "integer", "character varying(1000)", "character varying(1200)", "character varying(200)", "timestamp with time zone", "character varying(32)", "character varying(200)", "character varying(300)", "character varying(32)", "timestamp with time zone", "uuid" },
            values: new object[,]
            {
                { new Guid("44444444-4444-4444-4444-444444444441"), seedDate, 2, "Learning from real content helps vocabulary stick.", "A set of words known or used by a person.", "vocabulary", seedDate, "Learning", "vocabulary", "словниковий запас", "Word", seedDate, new Guid("11111111-1111-1111-1111-111111111111") },
                { new Guid("44444444-4444-4444-4444-444444444442"), seedDate, 1, "Learning from real content helps vocabulary stick.", "Texts, subtitles, videos, and examples created for real communication.", "real content", seedDate.AddDays(2), "Known", "real content", "реальний контент", "Phrase", seedDate, new Guid("11111111-1111-1111-1111-111111111111") }
            });

        migrationBuilder.InsertData(
            table: "ExportJobs",
            columns: new[] { "Id", "CompletedAt", "CreatedAt", "FileName", "FilterStatus", "Format", "Payload", "RowCount", "Status", "UserId" },
            columnTypes: new[] { "uuid", "timestamp with time zone", "timestamp with time zone", "character varying(240)", "character varying(32)", "character varying(16)", "text", "integer", "character varying(32)", "uuid" },
            values: new object[] { new Guid("77777777-7777-7777-7777-777777777771"), seedDate, seedDate, "vocabminer-export-seed.tsv", null, "Tsv", "Term\tTranslation\tExplanation\tExample\nvocabulary\tсловниковий запас\tA set of words known or used by a person.\tLearning from real content helps vocabulary stick.", 2, "Completed", new Guid("11111111-1111-1111-1111-111111111111") });

        migrationBuilder.InsertData(
            table: "Segments",
            columns: new[] { "Id", "ContentSourceId", "CreatedAt", "EndTime", "Position", "StartTime", "Text" },
            columnTypes: new[] { "uuid", "uuid", "timestamp with time zone", "interval", "integer", "interval", "text" },
            values: new object[,]
            {
                { new Guid("33333333-3333-3333-3333-333333333331"), new Guid("22222222-2222-2222-2222-222222222222"), seedDate, TimeSpan.FromSeconds(4), 1, TimeSpan.Zero, "Learning from real content helps vocabulary stick." },
                { new Guid("33333333-3333-3333-3333-333333333332"), new Guid("22222222-2222-2222-2222-222222222222"), seedDate, TimeSpan.FromSeconds(9), 2, TimeSpan.FromSeconds(5), "A learner saves useful phrases with context." }
            });

        migrationBuilder.InsertData(
            table: "Occurrences",
            columns: new[] { "Id", "CharacterEnd", "CharacterStart", "ContentSourceId", "ContextAfter", "ContextBefore", "ContextText", "CreatedAt", "LearningUnitId", "SegmentId" },
            columnTypes: new[] { "uuid", "integer", "integer", "uuid", "character varying(1000)", "character varying(1000)", "character varying(1000)", "timestamp with time zone", "uuid", "uuid" },
            values: new object[] { new Guid("55555555-5555-5555-5555-555555555551"), 43, 33, new Guid("22222222-2222-2222-2222-222222222222"), "stick.", "Learning from real content helps", "vocabulary", seedDate, new Guid("44444444-4444-4444-4444-444444444441"), new Guid("33333333-3333-3333-3333-333333333331") });

        migrationBuilder.InsertData(
            table: "ReviewAttempts",
            columns: new[] { "Id", "Grade", "LearningUnitId", "NextDueAt", "Notes", "ResponseTimeSeconds", "ReviewedAt", "UserId" },
            columnTypes: new[] { "uuid", "character varying(32)", "uuid", "timestamp with time zone", "character varying(500)", "integer", "timestamp with time zone", "uuid" },
            values: new object[] { new Guid("66666666-6666-6666-6666-666666666661"), "Good", new Guid("44444444-4444-4444-4444-444444444442"), seedDate.AddDays(2), "Seed review attempt.", 12, seedDate.AddDays(-1), new Guid("11111111-1111-1111-1111-111111111111") });

        migrationBuilder.CreateIndex(
            name: "IX_ContentSources_UserId_Title",
            table: "ContentSources",
            columns: new[] { "UserId", "Title" });

        migrationBuilder.CreateIndex(
            name: "IX_ExportJobs_UserId_CreatedAt",
            table: "ExportJobs",
            columns: new[] { "UserId", "CreatedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_LearningUnits_UserId_NormalizedTerm",
            table: "LearningUnits",
            columns: new[] { "UserId", "NormalizedTerm" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Occurrences_ContentSourceId",
            table: "Occurrences",
            column: "ContentSourceId");

        migrationBuilder.CreateIndex(
            name: "IX_Occurrences_LearningUnitId_SegmentId",
            table: "Occurrences",
            columns: new[] { "LearningUnitId", "SegmentId" });

        migrationBuilder.CreateIndex(
            name: "IX_Occurrences_SegmentId",
            table: "Occurrences",
            column: "SegmentId");

        migrationBuilder.CreateIndex(
            name: "IX_ReviewAttempts_LearningUnitId",
            table: "ReviewAttempts",
            column: "LearningUnitId");

        migrationBuilder.CreateIndex(
            name: "IX_ReviewAttempts_UserId_ReviewedAt",
            table: "ReviewAttempts",
            columns: new[] { "UserId", "ReviewedAt" });

        migrationBuilder.CreateIndex(
            name: "IX_Segments_ContentSourceId_Position",
            table: "Segments",
            columns: new[] { "ContentSourceId", "Position" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_Email",
            table: "Users",
            column: "Email",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ExportJobs");
        migrationBuilder.DropTable(name: "Occurrences");
        migrationBuilder.DropTable(name: "ReviewAttempts");
        migrationBuilder.DropTable(name: "Segments");
        migrationBuilder.DropTable(name: "LearningUnits");
        migrationBuilder.DropTable(name: "ContentSources");
        migrationBuilder.DropTable(name: "Users");
    }
}
