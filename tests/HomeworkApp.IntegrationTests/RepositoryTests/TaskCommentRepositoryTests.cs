using FluentAssertions;
using HomeworkApp.Dal.Entities;
using HomeworkApp.Dal.Models;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.IntegrationTests.Creators;
using HomeworkApp.IntegrationTests.Fakers;
using HomeworkApp.IntegrationTests.Fixtures;
using Microsoft.VisualBasic;
using StackExchange.Redis;
using Xunit;

namespace HomeworkApp.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class TaskCommentRepositoryTests
{
    private readonly ITaskCommentRepository _repository;

    public TaskCommentRepositoryTests(TestFixture fixture)
    {
        _repository = fixture.TaskCommentRepository;
    }

    [Fact]
    public async Task Add_TaskComment_Success()
    {
        // Arrange
        const int count = 5;

        var taskComments = TaskCommentEntityV1Faker.Generate(count);
        
        // Act
        var results = new List<long>();
        foreach (var comment in taskComments)
        {
            results.Add(await _repository.Add(comment, default));
        }

        // Asserts
        results.Should().HaveCount(count);
        results.Should().OnlyContain(x => x > 0);
    }

    [Fact]
    public async Task Update_TaskComment_Success()
    {
        // Arrange
        const string newMessage = "testUpdate";
        var taskComments = TaskCommentEntityV1Faker.Generate();
        var taskCommentId = await _repository.Add(taskComments.First(), default);
        var expectedTaskComment = taskComments.First()
            .WithId(taskCommentId)
            .WithMessage(newMessage);
        
        // Act
        await _repository.Update(expectedTaskComment, default);
        var result = await _repository.Get(new TaskCommentGetModel(){
            IncludeDeleted = false,
            TaskId = expectedTaskComment.TaskId
        }, default);
        // Asserts
        
        Assert.Equal(result.First().Id, expectedTaskComment.Id);
        Assert.Equal(result.First().TaskId, expectedTaskComment.TaskId);
        Assert.Equal(result.First().Message, expectedTaskComment.Message);
        result.First().ModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task SetDeleted_TaskComment_Success()
    {
        // Arrange
        const string newMessage = "testUpdate";
        var taskComments = TaskCommentEntityV1Faker.Generate();
        var taskCommentId = await _repository.Add(taskComments.First(), default);
        var expectedTaskComment = taskComments.First()
            .WithId(taskCommentId);
        
        // Act
        await _repository.SetDeleted(taskCommentId, default);
        var result = await _repository.Get(new TaskCommentGetModel(){
            IncludeDeleted = true,
            TaskId = expectedTaskComment.TaskId
        }, default);
        // Asserts
        
        Assert.Equal(result.First().Id, expectedTaskComment.Id);
        Assert.Equal(result.First().TaskId, expectedTaskComment.TaskId);
        Assert.Equal(result.First().Message, expectedTaskComment.Message);
        result.First().DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Get_TaskComment_Success()
    {
        // Arrange
        const int count = 5;
        var taskComments = TaskCommentEntityV1Faker.Generate(count);
        
        var expectedComments = new List<TaskCommentEntityV1>();
        foreach (var comment in taskComments)
        {
            var expectedCommentId = await _repository.Add(comment.WithTaskId(1), default);
            expectedComments.Add(comment.WithId(expectedCommentId).WithTaskId(1));
        }
        
        // Act
        var results = await _repository.Get(new TaskCommentGetModel()
        {
            TaskId = 1,
            IncludeDeleted = false,
        }, default);
        
        // Asserts
        results.Should().HaveCount(count);
        foreach (var comment in results)
        {
            Assert.Equal(comment.Id, expectedComments[(int)(comment.Id - 1)].Id);
            Assert.Equal(comment.TaskId, expectedComments[(int)(comment.Id - 1)].TaskId);
            Assert.Equal(comment.Message, expectedComments[(int)(comment.Id - 1)].Message);
        }
    }
}