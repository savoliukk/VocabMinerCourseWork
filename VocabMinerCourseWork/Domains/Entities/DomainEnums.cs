namespace VocabMinerCourseWork.Api.Domains.Entities;

public enum ContentSourceType
{
    PlainText = 1,
    Subtitle = 2,
    Article = 3
}

public enum LearningUnitType
{
    Word = 1,
    Phrase = 2
}

public enum LearningStatus
{
    New = 1,
    Learning = 2,
    Known = 3,
    Mastered = 4,
    Ignored = 5
}

public enum ReviewGrade
{
    Again = 1,
    Hard = 2,
    Good = 3,
    Easy = 4
}

public enum ExportFormat
{
    Csv = 1,
    Tsv = 2
}

public enum ExportStatus
{
    Pending = 1,
    Completed = 2,
    Failed = 3
}
