using Dapper;
using HomeworkApp.Dal.Entities;
using HomeworkApp.Dal.Models;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.Dal.Settings;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.CompilerServices;

namespace HomeworkApp.Dal.Repositories;

public class TaskCommentRepository: PgRepository, ITaskCommentRepository
{
    public TaskCommentRepository(IOptions<DalOptions> dalSettings) : base(dalSettings.Value)
    {
    }

    public async Task<long> Add(TaskCommentEntityV1 comment, CancellationToken token)
    {
        const string sqlQuery = @"
insert into task_comments (task_id, author_user_id, message, at) 
    values (@TaskIdComment, @AuthorUserIdComment, @MessageComment, @AtComment)
returning id;
";

        await using var connection = await GetConnection();
        var id = await connection.QueryAsync<long>(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    TaskIdComment = comment.TaskId,
                    AuthorUserIdComment = comment.AuthorUserId,
                    MessageComment = comment.Message,
                    AtComment = comment.At,
                },
                cancellationToken: token));
        return id.First();
    }

    public async Task Update(TaskCommentEntityV1 comment, CancellationToken token)
    {
        const string sqlQuery = @"
UPDATE task_comments SET (modified_at, message) = (@ModifiedAt, @NewMessage)
    WHERE id = @CommentId
";
        var @params = new DynamicParameters();
        
        @params.Add($"ModifiedAt", DateTimeOffset.UtcNow);
        @params.Add($"NewMessage", comment.Message);
        @params.Add($"CommentId", comment.Id);

        await using var connection = await GetConnection();
        var cmd = new CommandDefinition(
            sqlQuery,
            @params,
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: token);
        await connection.QueryAsync(cmd);
    }

    public async Task SetDeleted(long taskCommentId, CancellationToken token)
    {
        const string sqlQuery = @"
UPDATE task_comments SET deleted_at = @DeletedAt
    WHERE id = @CommentId
";
        var @params = new DynamicParameters();
        
        @params.Add($"DeletedAt", DateTimeOffset.UtcNow);
        @params.Add($"CommentId", taskCommentId);

        await using var connection = await GetConnection();
        var cmd = new CommandDefinition(
            sqlQuery,
            @params,
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: token);
        await connection.QueryAsync(cmd);
    }

    public async Task<TaskCommentEntityV1[]> Get(TaskCommentGetModel query, CancellationToken token)
    {
        var baseSql = @"
select id
     , task_id
     , author_user_id
     , message
     , at
     , modified_at
     , deleted_at
  from task_comments
";
        
        var conditions = new List<string>();
        var @params = new DynamicParameters();

        conditions.Add($"task_id = @TaskId");
        @params.Add($"TaskId", query.TaskId);
        if (query.IncludeDeleted)
        {
            conditions.Add($"deleted_at is not null");
        }
        else
        {
            conditions.Add($"deleted_at is null");
        }

        baseSql = baseSql + $" WHERE {string.Join(" AND ", conditions)} ORDER BY at DESC";
        
        var cmd = new CommandDefinition(
            baseSql,
            @params,
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: token);
        
        await using var connection = await GetConnection();
        return (await connection.QueryAsync<TaskCommentEntityV1>(cmd))
            .ToArray();
    }
}