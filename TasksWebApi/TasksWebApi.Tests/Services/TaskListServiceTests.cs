using Microsoft.EntityFrameworkCore;
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
public class TaskListServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITaskListRepository> _taskListRepositoryMock;
    private readonly Mock<IHttpContextService> _httpContextServiceMock;
    private TaskListService _taskListService;
    
    public TaskListServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _taskListRepositoryMock = new Mock<ITaskListRepository>();
        _httpContextServiceMock = new Mock<IHttpContextService>();
    }
    
    [TestInitialize]
    public Task InitializeAsync()
    {
        _taskListRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string userId, int pageSize, int pageNumber, CancellationToken cancellationToken) =>
            {
                List<object> auxList = new();
                for (int i = 0; i < 7; i++)
                    auxList.Add(new());
                
                var paginatedAuxList = auxList
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);
                
                return GivenTaskLists(paginatedAuxList.Count());
            });
        
        _taskListRepositoryMock
            .Setup(x => x.GetTotalRecordsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(7);
        
        _taskListRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))!
            .ReturnsAsync((int id, CancellationToken cancellationToken) =>
            {
                if (id < 7)
                    return GivenTaskList(id);
                
                return null;
            });
        
        _taskListRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<TaskListEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _taskListRepositoryMock
            .Setup(x => x.AttachAsync(It.IsAny<int>(), It.IsAny<byte[]>()))
            .ReturnsAsync((int id, byte[] rowVersion) => new TaskListEntity { Id = id, RowVersion = rowVersion});
        
        _taskListRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .Returns((int id, byte[] rowVersion, CancellationToken cancellationToken) =>
            {
                if (rowVersion.SequenceEqual(new byte[] { 0x01 }))
                    return Task.CompletedTask;
                else
                    throw new DbUpdateConcurrencyException();
            });
        
        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        
        _httpContextServiceMock
            .Setup(x => x.GetContextUser())
            .Returns(new UserResponse(Guid.NewGuid().ToString(), "user1", "user1@dicres.com", new List<string>()));

        _taskListService = new TaskListService(_unitOfWorkMock.Object, _taskListRepositoryMock.Object, _httpContextServiceMock.Object);
        return Task.CompletedTask;
    }

    [TestMethod]
    [DataRow(2, 1, 2)]
    [DataRow(2, 4, 1)]
    [DataRow(2, 5, 0)]
    public async Task get_all_task_lists(int pageSize, int pageNumber, int expectedCount)
    {
        PaginationResponse<ReadTaskListResponse> result = await _taskListService.GetAllAsync(new PaginationRequest(pageSize, pageNumber));

        Assert.AreEqual(7, result.TotalRegisters);
        Assert.AreEqual(expectedCount, result.Result.Count());
    }
    
    [TestMethod]
    [DataRow(2, false)]
    [DataRow(8, true)]
    public async Task get_task_lists(int id, bool expectedNull)
    {
        TaskListEntity givenTaskListsList = GivenTaskList(id);
        ReadTaskListResponse taskList = await _taskListService.GetAsync(id);

        if (expectedNull)
            Assert.AreEqual(null, taskList);
        else
        {
            Assert.AreEqual(givenTaskListsList.Id, taskList.Id);
            Assert.AreEqual(givenTaskListsList.Name, taskList.Name);
        }
    }
    
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task add_task_list(bool existPrevious)
    {
        _taskListRepositoryMock
            .Setup(x => x.ExistsOtherListWithSameNameAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existPrevious);
        
        _httpContextServiceMock
            .Setup(x => x.GetContextUser())
            .Returns(new UserResponse("1", "jorge", "jorge@jorge.dicres", new List<string>()));
        
        CreateTaskListRequest taskList = new("List 2");
        
        if (existPrevious)
            await Assert.ThrowsExceptionAsync<NotValidOperationException>(() => _taskListService.AddAsync(taskList), ErrorCodes.TASK_LIST_NAME_EXISTS);
        else
        {
            int taskListId = await _taskListService.AddAsync(taskList);
            Assert.IsInstanceOfType(taskListId, typeof(int));
        }
    }
    
    [TestMethod]
    [DataRow(false, false, false)]
    [DataRow(false, true, false)]
    [DataRow(true, false, false)]
    [DataRow(true, true, false)]
    [DataRow(false, true, true)]
    public async Task update_task_list(bool existsPrevious, bool ownsTheList, bool concurrencyError)
    {
        _taskListRepositoryMock
            .Setup(x => x.ExistsOtherListWithSameNameAsync(It.IsAny<string>(),It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existsPrevious);
        
        int taskId = 2;
        _httpContextServiceMock
            .Setup(x => x.GetContextUser())
            .Returns(() =>
            {
                if (ownsTheList)
                    return new UserResponse(taskId.ToString(), "jorge", "jorge@jorge.dicres", new List<string>());
                
                return new UserResponse($"{taskId + 1}", "jorge", "jorge@jorge.dicres", new List<string>());
            });
        var rowVersion = concurrencyError ? new byte[] { 0x02 } : new byte[] { 0x01 };
        UpdateTaskListRequest taskList = new(taskId, rowVersion, "List updated");
        if (existsPrevious)
            await Assert.ThrowsExceptionAsync<NotValidOperationException>(() => _taskListService.UpdateAsync(taskList), ErrorCodes.TASK_LIST_NAME_EXISTS);
        else if (!ownsTheList)
            await Assert.ThrowsExceptionAsync<ForbidenActionException>(() => _taskListService.UpdateAsync(taskList));
        else if (concurrencyError)
            await Assert.ThrowsExceptionAsync<DbUpdateConcurrencyException>(() => _taskListService.UpdateAsync(taskList));
        else
        {
            Task result = _taskListService.UpdateAsync(taskList);
            Assert.AreEqual(Task.CompletedTask, result);
        }
    }
    
    [TestMethod]
    [DataRow(8, false, false, false)]
    [DataRow(3, true, true, false)]
    [DataRow(3, true, false, false)]
    [DataRow(3, false, true, false)]
    [DataRow(3, false, false, false)]
    [DataRow(3, false, true, true)]
    public async Task delete_task_list(int id, bool containsAnyTask, bool ownsTheList, bool concurrencyError)
    {
        TaskListEntity givenTaskList = GivenTaskList(id);
        
        _taskListRepositoryMock
            .Setup(x => x.ContainsAnyTaskAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(containsAnyTask);
        
        _httpContextServiceMock
            .Setup(x => x.GetContextUser())
            .Returns(() =>
            {
                if (ownsTheList)
                    return new UserResponse(id.ToString(), "jorge", "jorge@jorge.dicres", new List<string>());
                
                return new UserResponse($"{id + 1}", "jorge", "jorge@jorge.dicres", new List<string>());
            });
        
        bool existsTaskList = id <= 7;
        if (existsTaskList && !containsAnyTask && ownsTheList && !concurrencyError)
            await _taskListService.DeleteAsync(new DeleteRequest(givenTaskList.Id, new byte[]{0x01}));
        
        if (concurrencyError)
            await Assert.ThrowsExceptionAsync<DbUpdateConcurrencyException>(() => _taskListService.DeleteAsync(new DeleteRequest(givenTaskList.Id, new byte[]{0x02})));
        else if (!existsTaskList)
            await Assert.ThrowsExceptionAsync<NotValidOperationException>(() => _taskListService.DeleteAsync(new DeleteRequest(givenTaskList.Id, new byte[]{})), ErrorCodes.ITEM_NOT_EXISTS);
        else if (containsAnyTask)
            await Assert.ThrowsExceptionAsync<NotValidOperationException>(() => _taskListService.DeleteAsync(new DeleteRequest(givenTaskList.Id, new byte[]{})), ErrorCodes.TASK_LIST_WITH_TASKS);
        else if (!ownsTheList)
            await Assert.ThrowsExceptionAsync<ForbidenActionException>(() => _taskListService.DeleteAsync(new DeleteRequest(givenTaskList.Id, new byte[]{})));
    }
    
    private List<TaskListEntity> GivenTaskLists(int count)
    {
        var result = new List<TaskListEntity>();

        for (int i = 1; i <= count; i++)
            result.Add(new() { Name = $"List {i}" });

        return result;
    }
    
    private TaskListEntity GivenTaskList(int taskListId)
    {
        return new TaskListEntity { Id = taskListId, RowVersion = new byte[]{ 0x01 }, Name = $"List {taskListId}", UserId = $"{taskListId}"};
    }
}