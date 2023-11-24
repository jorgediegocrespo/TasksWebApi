using Microsoft.EntityFrameworkCore;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.Extensions;

namespace TasksWebApi.DataAccess.Repositories;

public class TaskRepository : BaseRepository<TaskEntity>,  ITaskRepository
{
    public TaskRepository(TasksDbContext dbContext) : base(dbContext)
    { }

    protected override DbSet<TaskEntity> DbEntity => dbContext.Tasks;
    
    public async Task<int> GetTotalRecordsAsync(int taskListId, CancellationToken cancellationToken = default)
    {
        return await DbEntity.Where(x => x.TaskListId == taskListId).CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskEntity>> GetAllAsync(int taskListId, int pageSize, int pageNumber, CancellationToken cancellationToken = default)
    {
        var result = await DbEntity
            .Where(x => x.TaskListId == taskListId)
            .OrderBy(x => x.Description)
            .Paginate(pageSize, pageNumber)
            .ToListAsync(cancellationToken);
        dbContext.ChangeTracker.Clear();

        return result;
    }

    public async Task<TaskEntity> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var result = await DbEntity.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        dbContext.ChangeTracker.Clear();

        return result;
    }

    public async Task AddAsync(TaskEntity entity, CancellationToken cancellationToken = default)
    {
        await DbEntity.AddAsync(entity, cancellationToken);
    }

    public Task<TaskEntity> AttachAsync(int id, byte[] rowVersion)
    {
        TaskEntity entity = new TaskEntity { Id = id, RowVersion = rowVersion};
        DbEntity.Attach(entity);

        return Task.FromResult(entity);
    }

    public async Task DeleteAsync(int id, byte[] rowVersion, CancellationToken cancellationToken = default)
    {
        await DbEntity.RemoveConcurrentlyAsync(id, rowVersion, cancellationToken);
    }

    public async Task DeleteAllAsync(int taskListId, CancellationToken cancellationToken = default)
    {
        var toRemove = await DbEntity.Where(x => x.TaskListId == taskListId).ToListAsync(cancellationToken);
        DbEntity.RemoveRange(toRemove);
    }
}