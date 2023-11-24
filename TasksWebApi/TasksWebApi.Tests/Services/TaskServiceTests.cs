using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TasksWebApi.Constants;
using TasksWebApi.DataAccess;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.DataAccess.Repositories;
using TasksWebApi.Exceptions;
using TasksWebApi.Models;
using TasksWebApi.Services;

namespace TasksWebApi.Tests.Services;

[TestClass]
public class TaskServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<ITaskListRepository> _taskListRepositoryMock;
    private readonly Mock<IHttpContextService> _httpContextServiceMock;
    private TaskService _taskService;

    public TaskServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _taskListRepositoryMock = new Mock<ITaskListRepository>();
        _httpContextServiceMock = new Mock<IHttpContextService>();
    }
    
    [TestInitialize]
    public Task InitializeAsync()
    {
        //Task list 1 has 2 tasks
        //Task list 2 has 7 tasks
        //Task list 3 has 0 tasks
        
        _taskRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int taskListId, int pageSize, int pageNumber, CancellationToken cancellationToken) =>
            {
                int tasksInList = taskListId switch
                {
                    1 => 2,
                    2 => 7,
                    _ => 0
                };
                if (tasksInList == 0)
                    return GivenTasks(0);
                
                List<object> auxList = new();
                for (int i = 0; i < tasksInList; i++)
                    auxList.Add(new());
                
                var paginatedAuxList = auxList
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);
                
                return GivenTasks(paginatedAuxList.Count());
            });
        
        _taskRepositoryMock
            .Setup(x => x.GetTotalRecordsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int taskListId, CancellationToken cancellationToken) =>
            {
                return taskListId switch
                {
                    1 => 2,
                    2 => 7,
                    _ => 0
                };
            });
        
        _taskRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync((int id, CancellationToken cancellationToken) =>
            {
                if (id < 10)
                    return GivenTask(id);
                
                return null;
            });
        
        _taskRepositoryMock
            .Setup(x => x.AttachAsync(It.IsAny<int>(), It.IsAny<byte[]>()))
            .ReturnsAsync((int id, byte[] rowVersion) => new TaskEntity { Id = id, RowVersion = rowVersion});
        
        _taskListRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync((int id, CancellationToken cancellationToken) =>
            {
                if (id > 3)
                    return null;
                return new TaskListEntity() { Id = id, RowVersion = new []{(byte)id}};
            });
        
        _httpContextServiceMock
            .Setup(x => x.GetContextUser())
            .Returns(new UserResponse(Guid.NewGuid().ToString(), "user1", "user1@dicres.com", new List<string>()));
        
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        
        _taskService = new TaskService(_unitOfWorkMock.Object, _taskRepositoryMock.Object, _taskListRepositoryMock.Object, _httpContextServiceMock.Object);
        return Task.CompletedTask;
    }
    
    [TestMethod]
    [DataRow(1, 2, 1, 2, 2)]
    [DataRow(1, 5, 1, 2, 2)]
    [DataRow(1, 2, 2, 2, 0)]
    [DataRow(2, 2, 1, 7, 2)]
    [DataRow(2, 2, 4, 7, 1)]
    [DataRow(2, 2, 5, 7, 0)]
    [DataRow(3, 2, 1, 0, 0)]
    public async Task get_all_tasks(int taskListId, int pageSize, int pageNumber, int expectedTotalRegister, int expectedCount)
    {
        PaginationResponse<ReadTaskResponse> result = await _taskService.GetAllAsync(new TaskPaginationRequest(taskListId, pageSize, pageNumber));

        Assert.AreEqual(expectedTotalRegister, result.TotalRegisters);
        Assert.AreEqual(expectedCount, result.Result.Count());
    }
    
    [TestMethod]
    [DataRow(2, false)]
    [DataRow(10, true)]
    public async Task get_task(int id, bool expectedNull)
    {
        TaskEntity givenTask = GivenTask(id);
        ReadTaskResponse task = await _taskService.GetAsync(id);

        if (expectedNull)
            Assert.AreEqual(null, task);
        else
        {
            Assert.AreEqual(givenTask.Id, task.Id);
            Assert.AreEqual(givenTask.Description, task.Description);
            Assert.AreEqual(givenTask.Notes, task.Notes);
        }
    }
    
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task add_task(bool existList)
    {
        _taskListRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(existList);
        
        CreateTaskRequest taskList = new(1, "List 1", "This is list 1");
        
        if (!existList)
            await Assert.ThrowsExceptionAsync<NotValidOperationException>(() => _taskService.AddAsync(taskList), ErrorCodes.TASK_LIST_NOT_EXISTS);
        else
        {
            int taskListId = await _taskService.AddAsync(taskList);
            Assert.IsInstanceOfType(taskListId, typeof(int));
        }
    }
    
    [TestMethod]
    [DataRow(1, true, true)]
    [DataRow(1, false, true)]
    [DataRow(10, true, false)]
    [DataRow(10, false, false)]
    public async Task update_task(int id, bool existList, bool expectedExistTask)
    {
        _taskListRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync(existList);
        
        UpdateTaskRequest taskList = new(id, new []{(byte)id}, 1, "List 1", "This is list 1");
        
        if (!expectedExistTask)
            await Assert.ThrowsExceptionAsync<NotValidOperationException>(() => _taskService.UpdateAsync(taskList), ErrorCodes.ITEM_NOT_EXISTS);
        else if (!existList)
            await Assert.ThrowsExceptionAsync<NotValidOperationException>(() => _taskService.UpdateAsync(taskList), ErrorCodes.TASK_LIST_NOT_EXISTS);
        else
            await _taskService.UpdateAsync(taskList);
    }
    
    [TestMethod]
    [DataRow(1, true)]
    [DataRow(12, false)]
    public async Task delete_task(int id, bool expectedExists)
    {
        if (expectedExists)
            await _taskService.DeleteAsync(new DeleteRequest(id, new []{(byte)id}));
        else
            await Assert.ThrowsExceptionAsync<NotValidOperationException>(() => _taskService.DeleteAsync(new DeleteRequest(id, new []{(byte)id})), ErrorCodes.ITEM_NOT_EXISTS);
    }
    
    [TestMethod]
    [DataRow(1, true)]
    [DataRow(3, true)]
    [DataRow(4, false)]
    public async Task delete_all_in_list_task(int taskListId, bool expectedExists)
    {
        if (expectedExists)
            await _taskService.DeleteAllInListAsync(new DeleteRequest(taskListId, new []{(byte)taskListId}));
        else
            await Assert.ThrowsExceptionAsync<NotValidOperationException>(() => _taskService.DeleteAllInListAsync(new DeleteRequest(taskListId, new byte[]{(byte)taskListId})), ErrorCodes.ITEM_NOT_EXISTS);
    }
    
    private List<TaskEntity> GivenTasks(int count)
    {
        var result = new List<TaskEntity>();

        for (int i = 1; i <= count; i++)
            result.Add(new() { Description = $"Task {i}" });

        return result;
    }
    
    private TaskEntity GivenTask(int taskId)
    {
        return new TaskEntity { Id = taskId, RowVersion = new [] {(byte)taskId}, Description = $"List {taskId}" };
    }
}