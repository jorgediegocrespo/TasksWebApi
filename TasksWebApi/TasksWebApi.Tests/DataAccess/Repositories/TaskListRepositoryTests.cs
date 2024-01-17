using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TasksWebApi.DataAccess;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.DataAccess.Repositories;

namespace TasksWebApi.Tests.DataAccess.Repositories;

[TestClass]
public class TaskListRepositoryTests : BaseRepositoryTests
{
    private TasksDbContext _context;
    private TaskListRepository _repository;
    private string user1Id;
    private string user2Id;
    
    [TestInitialize]
    public async Task InitializeAsync()
    {
        _context = await GetLocalTasksDbContextAsync(Guid.NewGuid().ToString());
        await _context.Users.AddAsync(new UserEntity()
        {
            UserName = "user1",
            Email = "user1@dicres.com"
        });
        await _context.Users.AddAsync(new UserEntity()
        {
            UserName = "user2",
            Email = "user2@dicres.com"
        });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        user1Id = await _context.Users.Where(x => x.UserName == "user1").Select(x => x.Id).FirstAsync();
        user2Id = await _context.Users.Where(x => x.UserName == "user2").Select(x => x.Id).FirstAsync();
        await _context.TaskLists.AddRangeAsync(new List<TaskListEntity>
        {
            new() { Name = "List 1", UserId = user1Id }, //1
            new() { Name = "List 3", UserId = user1Id }, //2
            new() { Name = "List 5", UserId = user1Id }, //3
            new() { Name = "List 7", UserId = user1Id }, //4
            new() { Name = "List 12", UserId = user1Id, IsDeleted = true }, //5
            new() { Name = "List 6", UserId = user1Id }, //6
            new() { Name = "List 4", UserId = user1Id }, //7
            new()
            {
                Name = "List 2", UserId = user1Id, Tasks = new List<TaskEntity>() //8
                {
                    new() { Description = "Task 1", Notes = "This is the task 1" }, //1
                    new() { Description = "Task 2", Notes = "This is the task 2" }, //2
                }
            },
            new() { Name = "List 102", UserId = user2Id }, //9
            new()
            {
                Name = "List 101", UserId = user2Id, Tasks = new List<TaskEntity>() //10
                {
                    new() { Description = "Task 3", Notes = "This is the task 3" }, //3
                    new() { Description = "Task 4", Notes = "This is the task 4" }, //4
                    new() { Description = "Task 5", Notes = "This is the task 5" }, //5
                    new() { Description = "Task 6", Notes = "This is the task 6", IsDeleted = true }, //6
                }
            }
        });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        _repository = new TaskListRepository(_context);
    }
    
    [TestCleanup]
    public async Task CleanupAsync()
    {
        await DeleteDatabaseAsync(_context);
    }

    [TestMethod]
    [DataRow("List 1", 8, true)]
    [DataRow("list 1", 8, true)]
    [DataRow("LIST 1", 8, true)]
    [DataRow("  List 1  ", 8, true)]
    [DataRow("  List 1  ", 8, true)]
    [DataRow("List 1", 1, false)]
    [DataRow("list 1", 1, false)]
    [DataRow("LIST 1", 1, false)]
    [DataRow("  List 1  ", 1, false)]
    [DataRow("  List 1  ", 1, false)]
    [DataRow("List 8", 8, false)]
    [DataRow("List 1", 2, true)]
    public async Task exists_other_list_with_same_name_task_list(string newListName, int newListId, bool expectedResult)
    {
        var result = await _repository.ExistsOtherListWithSameNameAsync(user1Id, newListId, newListName);
        Assert.AreEqual(expectedResult, result);
    }
    
    [TestMethod]
    [DataRow(9, false)]
    [DataRow(2, true)]
    public async Task exists_task_list(int listId, bool expectedResult)
    {
        var result = await _repository.ExistsAsync(user1Id, listId);
        Assert.AreEqual(expectedResult, result);
    }
    
    [TestMethod]
    public async Task get_total_record_task_list()
    {
        var result = await _repository.GetTotalRecordsAsync(user1Id);
        Assert.AreEqual(7, result);
    }

    [TestMethod]
    [DataRow(2, 1, new[] { 1, 8 })]
    [DataRow(2, 2, new[] { 2, 7 })]
    [DataRow(2, 4, new[] { 4 })]
    public async Task get_all_task_list(int pageSize, int pageNumber, int[] hopeIds)
    {
        var result = await _repository.GetAllAsync(user1Id, pageSize, pageNumber);
        var resultList = result.ToList();

        Assert.AreEqual(hopeIds.Length, resultList.Count);
        for (var i = 0; i < resultList.Count; i++)
            Assert.AreEqual(hopeIds[i], resultList[i].Id);
    }

    [TestMethod]
    [DataRow(3, "List 5")]
    [DataRow(12, null)]
    public async Task get_task_list(int id, string expectedName)
    {
        var result = await _repository.GetAsync(id);
        Assert.AreEqual(expectedName, result?.Name);
    }
    
    [TestMethod]
    public async Task add_task_list()
    {
        await _repository.AddAsync(new TaskListEntity { Name = "List 8", UserId = user1Id});
        await _context.SaveChangesAsync();

        var count = await _context.TaskLists.Where(x => x.UserId == user1Id).CountAsync();
        Assert.AreEqual(count, 8);
    }
    
    [TestMethod]
    public async Task attach_task_list()
    {
        var dbTaskList = await _repository.GetAsync(4);
        var taskList = await _repository.AttachAsync(4, dbTaskList.RowVersion);
        taskList.Name = "List 8";
        
        await _context.SaveChangesAsync();

        var result = await _context.TaskLists.FirstOrDefaultAsync(x => x.Id == 4);
        Assert.AreEqual("List 8", result?.Name);
    }
    
    [TestMethod]
    [DataRow(4, true)]
    [DataRow(7, true)]
    [DataRow(5, false)]
    [DataRow(8, true)]
    public async Task delete_task_list(int id, bool removed)
    {
        var countBefore = await _context.TaskLists.Where(x => x.UserId == user1Id).CountAsync();
        var dbTaskList = await _repository.GetAsync(id);
        if (dbTaskList == null)
            return;
        
        await _repository.DeleteAsync(id, dbTaskList.RowVersion);
        await _context.SaveChangesAsync();
        
        var countAfter = await _context.TaskLists.Where(x => x.UserId == user1Id).CountAsync();
        Assert.AreEqual(countBefore, removed ? countAfter + 1 : countAfter);
    }
    
    [TestMethod]
    [DataRow(1, 8)]
    [DataRow(2, 2)]
    public async Task delete_by_user_task_list(int userId, int removedListCount)
    {
        var userIdString = userId switch
        {
            1 => user1Id,
            2 => user2Id,
            _ => throw new NotImplementedException()
        };
        var countBefore = await _context.TaskLists.IgnoreQueryFilters().Where(x => x.UserId == userIdString).CountAsync();
        await _repository.DeleteAsync(userIdString, true);
        await _context.SaveChangesWithoutSoftDeleteAsync();
        
        var countAfter = await _context.TaskLists.IgnoreQueryFilters().Where(x => x.UserId == userIdString).CountAsync();
        Assert.AreEqual(countBefore, countAfter + removedListCount);
    }
    
    [TestMethod]
    [DataRow(8, true)]
    [DataRow(3, false)]
    [DataRow(25, false)]
    public async Task contains_any_task_task_list(int taskListId, bool expectedResult)
    {
        var result = await _repository.ContainsAnyTaskAsync(taskListId);
        Assert.AreEqual(expectedResult, result);
    }
}