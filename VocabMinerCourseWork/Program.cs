using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using VocabMinerCourseWork.Api.BusinessLogic;
using VocabMinerCourseWork.Api.Data;
using VocabMinerCourseWork.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=vocabminer_coursework;Username=vocabminer;Password=vocabminer123";

builder.Services.AddDbContext<VocabMinerDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IContentSourceRepository, ContentSourceRepository>();
builder.Services.AddScoped<ISegmentRepository, SegmentRepository>();
builder.Services.AddScoped<ILearningUnitRepository, LearningUnitRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IExportRepository, ExportRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IContentImportService, ContentImportService>();
builder.Services.AddScoped<ILearningUnitService, LearningUnitService>();
builder.Services.AddScoped<IMockExplanationService, MockExplanationService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IExportService, ExportService>();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("ApplyMigrations"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<VocabMinerDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapGet("/", () => Results.Ok(new
{
    name = "VocabMiner CourseWork API",
    status = "running",
    docs = "/swagger"
}));

app.MapControllers();

app.Run();
