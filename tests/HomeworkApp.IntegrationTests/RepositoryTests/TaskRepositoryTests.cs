using FluentAssertions;
using HomeworkApp.Dal.Entities;
using HomeworkApp.Dal.Models;
using HomeworkApp.Dal.Repositories.Interfaces;
using HomeworkApp.IntegrationTests.Creators;
using HomeworkApp.IntegrationTests.Fakers;
using HomeworkApp.IntegrationTests.Fixtures;
using Microsoft.VisualBasic;
using Xunit;

namespace HomeworkApp.IntegrationTests.RepositoryTests;

[Collection(nameof(TestFixture))]
public class TaskRepositoryTests
{
    private readonly ITaskRepository _repository;

    public TaskRepositoryTests(TestFixture fixture)
    {
        _repository = fixture.TaskRepository;
    }

    [Fact]
    public async Task Add_Task_Success()
    {
        // Arrange
        const int count = 5;

        var tasks = TaskEntityV1Faker.Generate(count);
        
        // Act
        var results = await _repository.Add(tasks, default);

        // Asserts
        results.Should().HaveCount(count);
        results.Should().OnlyContain(x => x > 0);
    }
    
    [Fact]
    public async Task Get_SingleTask_Success()
    {
        // Arrange
        var tasks = TaskEntityV1Faker.Generate();
        var taskIds = await _repository.Add(tasks, default);
        var expectedTaskId = taskIds.First();
        var expectedTask = tasks.First()
            .WithId(expectedTaskId);
        
        // Act
        var results = await _repository.Get(new TaskGetModel()
        {
            TaskIds = new[] { expectedTaskId }
        }, default);
        
        // Asserts
        results.Should().HaveCount(1);
        var task = results.Single();

        task.Should().BeEquivalentTo(expectedTask);
    }
    
    [Fact]
    public async Task AssignTask_Success()
    {
        // Arrange
        var assigneeUserId = Create.RandomId();
        
        var tasks = TaskEntityV1Faker.Generate();
        var taskIds = await _repository.Add(tasks, default);
        var expectedTaskId = taskIds.First();
        var expectedTask = tasks.First()
            .WithId(expectedTaskId)
            .WithAssignedToUserId(assigneeUserId);
        var assign = AssignTaskModelFaker.Generate()
            .First()
            .WithTaskId(expectedTaskId)
            .WithAssignToUserId(assigneeUserId);
        
        // Act
        await _repository.Assign(assign, default);
        
        // Asserts
        var results = await _repository.Get(new TaskGetModel()
        {
            TaskIds = new[] { expectedTaskId }
        }, default);
        
        results.Should().HaveCount(1);
        var task = results.Single();
        
        expectedTask = expectedTask with {Status = assign.Status};
        task.Should().BeEquivalentTo(expectedTask);
    }
    
    [Fact]
    public async Task GetSubTasksInStatus_Success()
    {
        // Arrange
        var tasks = TaskEntityV1Faker.GenerateTree(15);
        
        var taskIds = await _repository.Add(tasks, default);
        var parentTaskId = taskIds.First();
        var statuses = new []{1, 2, 3, 4, 5};
        
        // Act
        var res = await _repository.GetSubTasksInStatus(parentTaskId, statuses, default);
        
        // Asserts
        res.Should().HaveCount(14);

        foreach (var subTask in res)
        {
            var pathParentId = new List<long>();
            for (var id = subTask.Id; id > 1;)
            {
                id /= 2;
                pathParentId.Add(id);
            }

            pathParentId.Reverse();
            Assert.Equal(subTask.ParentsIds, pathParentId.ToArray());

            Assert.Contains(subTask.Status, statuses);
        }
    }
}
