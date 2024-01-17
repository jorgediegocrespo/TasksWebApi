using Microsoft.EntityFrameworkCore;

namespace TasksWebApi.DataAccess.Repositories;

public abstract class BaseRepository<T>(TasksDbContext dbContext)
    where T : class
{
    protected readonly TasksDbContext dbContext = dbContext;
    protected abstract DbSet<T> DbEntity { get; }
}