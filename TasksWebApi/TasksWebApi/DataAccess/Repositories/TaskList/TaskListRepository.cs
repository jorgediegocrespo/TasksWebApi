using Microsoft.EntityFrameworkCore;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.Extensions;

namespace TasksWebApi.DataAccess.Repositories;

public class TaskListRepository : BaseRepository<TaskListEntity>,  ITaskListRepository
{
    public TaskListRepository(TasksDbContext dbContext) : base(dbContext)
    { }

    protected override DbSet<TaskListEntity> DbEntity => dbContext.TaskLists;
    
    public async Task<bool> ExistsOtherListWithSameNameAsync(string userId, int id, string name, CancellationToken cancellationToken = default)
    {
        return await DbEntity.Where(x => 
                x.Name.ToLower().Trim() == name.ToLower().Trim() &&
                x.UserId == userId &&
                x.Id != id)
            .AnyAsync(cancellationToken);
    }
    
    public async Task<bool> ExistsAsync(string userId, int id, CancellationToken cancellationToken = default)
    {
        return await DbEntity.Where(x => 
                x.Id == id &&
                x.UserId == userId)
            .AnyAsync(cancellationToken);
    }

    public async Task<int> GetTotalRecordsAsync(string userId, CancellationToken cancellationToken = default, bool includeDeleted = false)
    {
        if (includeDeleted)
            return await DbEntity
                .IgnoreQueryFilters()
                .Where(x => x.UserId == userId)
                .CountAsync(cancellationToken);
        
        return await DbEntity.Where(x => x.UserId == userId).CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskListEntity>> GetAllAsync(string userId, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
    {
        var result = await DbEntity
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .Paginate(pageSize, pageNumber)
            .ToListAsync(cancellationToken);
        
        dbContext.ChangeTracker.Clear();

        return result;
    }

    public async Task<TaskListEntity> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await DbEntity.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        dbContext.ChangeTracker.Clear();

        return result;
    }

    public async Task AddAsync(TaskListEntity entity, CancellationToken cancellationToken = default)
    {
        await DbEntity.AddAsync(entity, cancellationToken);
    }

    public Task<TaskListEntity> AttachAsync(int id, byte[] rowVersion)
    {
        TaskListEntity entity = new TaskListEntity { Id = id, RowVersion = rowVersion};
        DbEntity.Attach(entity);

        return Task.FromResult(entity);
    }

    public async Task DeleteAsync(int id, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        await DbEntity.RemoveConcurrentlyAsync(id, rowVersion, cancellationToken);
    }

    public Task<bool> ContainsAnyTaskAsync(int id, CancellationToken cancellationToken = default)
    {
        return DbEntity
            .Where(x => x.Id == id && x.Tasks.Any())
            .AnyAsync(cancellationToken);
    }
}