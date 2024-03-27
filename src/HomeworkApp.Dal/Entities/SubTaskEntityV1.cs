namespace HomeworkApp.Dal.Entities;

public record SubTaskEntityV1
{
    public required long Id { get; init; }
    public required string Title { get; init; }
    public required int Status { get; init; }
    public required long[] ParentsIds { get; init; }
}