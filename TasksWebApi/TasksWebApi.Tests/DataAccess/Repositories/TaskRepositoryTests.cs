using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TasksWebApi.DataAccess;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.DataAccess.Repositories;

namespace TasksWebApi.Tests.DataAccess.Repositories;

[TestClass]
public class TaskRepositoryTests : BaseRepositoryTests
{
    private TasksDbContext _context;
    private TaskRepository _repository;
    
    [TestInitialize]
    public async Task InitializeAsync()
    {
        _context = await GetLocalTasksDbContextAsync(Guid.NewGuid().ToString());
        await _context.TaskLists.AddRangeAsync(new List<TaskListEntity>
        {
            new()
            {
                Name = "List 1", Tasks = new List<TaskEntity>() //1
                {
                    new() { Description = "Task 1", Notes = "This is the task 1"}, //1
                    new() { Description = "Task 2", Notes = "This is the task 2"}, //2
                }
            },
            new()
            {
                Name = "List 2", Tasks = new List<TaskEntity>() //2
                {
                    new() { Description = "Task 3" }, //3
                    new() { Description = "Task 5", Notes = "This is the task 5" }, //4
                    new() { Description = "Task 7", Notes = "This is the task 7" }, //5
                    new() { Description = "Task 9" }, //6
                    new() { Description = "Task 10", IsDeleted = true}, //7
                    new() { Description = "Task 8" }, //8
                    new() { Description = "Task 6", Notes = "This is the task 6" }, //9
                    new() { Description = "Task 4" }, //10
                }
            },
            new() { Name = "List 3" }, //3
            new()
            {
                Name = "List 4", IsDeleted = true, Tasks = new List<TaskEntity>() //4
                {
                    new() { Description = "Task 1", Notes = "This is the task 1", IsDeleted = true }, //11
                    new() { Description = "Task 2", Notes = "This is the task 2", IsDeleted = true }, //12
                }
            },
        });
        
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        _repository = new TaskRepository(_context);
    }
    
    [TestCleanup]
    public async Task CleanupAsync()
    {
        await DeleteDatabaseAsync(_context);
    }
    
    [TestMethod]
    [DataRow(1, 2)]
    [DataRow(2, 7)]
    [DataRow(3, 0)]
    public async Task get_total_record_tasks(int taskListId, int expectedResult)
    {
        var result = await _repository.GetTotalRecordsAsync(taskListId);
        Assert.AreEqual(expectedResult, result);
    }
    
    [TestMethod]
    [DataRow(1, 3, 1, new[] { 1, 2 })]
    [DataRow(2, 2, 1, new[] { 3, 10 })]
    [DataRow(2, 2, 2, new[] { 4, 9 })]
    [DataRow(2, 2, 4, new[] { 6 })]
    [DataRow(3, 2, 1, new int[] {})]
    public async Task get_all_tasks(int taskListId, int pageSize, int pageNumber, int[] hopeIds)
    {
        var result = await _repository.GetAllAsync(taskListId, pageSize, pageNumber);
        var resultList = result.ToList();

        Assert.AreEqual(hopeIds.Length, resultList.Count);
        for (var i = 0; i < resultList.Count; i++)
            Assert.AreEqual(hopeIds[i], resultList[i].Id);
    }
    
    [TestMethod]
    [DataRow(2, "Task 2", "This is the task 2")]
    [DataRow(5, "Task 7", "This is the task 7")]
    [DataRow(6, "Task 9", null)]
    [DataRow(12, null, null)]
    public async Task get_task(int id, string expectedDescription, string expectedNotes)
    {
        var result = await _repository.GetAsync(id);
        Assert.AreEqual(expectedDescription, result?.Description);
        Assert.AreEqual(expectedNotes, result?.Notes);
    }
    
    [TestMethod]
    [DataRow(1, "Task 10", "This is the task 10")]
    [DataRow(2, "Task 10", null)]
    [DataRow(2, "Task 10", null)]
    public async Task add_task(int taskListId, string description, string notes)
    {
        var previousCount = await _context.Tasks.Where(x => x.TaskListId == taskListId).CountAsync();
        await _repository.AddAsync(new TaskEntity { TaskListId = taskListId, Description = description, Notes = notes});
        await _context.SaveChangesAsync();
        
        var count = await _context.Tasks.Where(x => x.TaskListId == taskListId).CountAsync();
        Assert.AreEqual(count, previousCount + 1);
    }
    
    [TestMethod]
    [DataRow(1, 2, "Task 10", "This is the task 10")]
    [DataRow(5, 3, "Task 10", "This is the task 10")]
    public async Task attach_task(int id, int taskListId, string description, string notes)
    {
        var dbTask = await _repository.GetAsync(id);
        var task = await _repository.AttachAsync(id, dbTask.RowVersion);
        task.Description = description;
        task.Notes = notes;
        task.TaskListId = taskListId;
        
        await _context.SaveChangesAsync();
    
        var result = await _context.Tasks.FirstOrDefaultAsync(x => x.Id == id);
        Assert.AreEqual(taskListId, result?.TaskListId);
        Assert.AreEqual(description, result?.Description);
        Assert.AreEqual(notes, result?.Notes);
    }
    
    [TestMethod]
    public async Task delete_task()
    {
        var dbTask = await _repository.GetAsync(4);
        await _repository.DeleteAsync(4, dbTask.RowVersion);
        await _context.SaveChangesAsync();
        
        var count = await _context.Tasks.CountAsync();
        Assert.AreEqual(count, 8);
    }
    
    [TestMethod]
    [DataRow(1, 7)]
    [DataRow(1, 2)]
    [DataRow(3, 9)]
    public async Task delete_task(int taskListId, int expectedTaskCount)
    {
        await _repository.DeleteAllAsync(taskListId);
        await _context.SaveChangesAsync();
        
        var count = await _context.Tasks.CountAsync();
        Assert.AreEqual(count, expectedTaskCount);
    }
}