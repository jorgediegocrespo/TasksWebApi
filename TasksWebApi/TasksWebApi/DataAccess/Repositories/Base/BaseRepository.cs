using Microsoft.EntityFrameworkCore;

namespace TasksWebApi.DataAccess.Repositories;

public abstract class BaseRepository<T>
    where T : class
{
    protected readonly TasksDbContext dbContext;
    protected abstract DbSet<T> DbEntity { get; }

    public BaseRepository(TasksDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
}