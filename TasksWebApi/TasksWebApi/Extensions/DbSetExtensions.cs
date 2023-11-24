using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TasksWebApi.DataAccess.Entities;

namespace TasksWebApi.Extensions;

public static class DbSetExtensions
{
    public static async Task<EntityEntry<T>> RemoveConcurrentlyAsync<T>(this DbSet<T> dbSet, int id, byte[] rowVersion, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var toRemove = await dbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
        if (toRemove?.RowVersion.SequenceEqual(rowVersion) != true)
            throw new DbUpdateConcurrencyException();

        return dbSet.Remove(toRemove);
    }
}