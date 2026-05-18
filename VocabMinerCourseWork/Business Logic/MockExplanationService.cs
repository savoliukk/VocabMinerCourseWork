using VocabMinerCourseWork.Api.Domains.ViewModels;

namespace VocabMinerCourseWork.Api.BusinessLogic;

public interface IMockExplanationService
{
    ExplanationResponse Generate(Guid learningUnitId, string term, string? context);
}

public class MockExplanationService : IMockExplanationService
{
    private static readonly Dictionary<string, string> UkrainianTranslations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["content"] = "контент",
        ["context"] = "контекст",
        ["vocabulary"] = "словниковий запас",
        ["review"] = "повторення",
        ["learning"] = "навчання",
        ["real content"] = "реальний контент"
    };

    public ExplanationResponse Generate(Guid learningUnitId, string term, string? context)
    {
        var normalized = LearningUnitService.NormalizeTerm(term);
        var translation = UkrainianTranslations.TryGetValue(normalized, out var value)
            ? value
            : $"навчальний переклад для \"{term}\"";
        var example = string.IsNullOrWhiteSpace(context)
            ? $"The term \"{term}\" was saved for later review."
            : context.Trim();

        return new ExplanationResponse
        {
            LearningUnitId = learningUnitId,
            Term = term,
            Translation = translation,
            Explanation = $"Mock-пояснення: \"{term}\" потрібно запам'ятати у конкретному контексті, а не як ізольований переклад.",
            ExampleSentence = example
        };
    }
}
