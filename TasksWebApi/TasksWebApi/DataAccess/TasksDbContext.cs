using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.DataAccess.EntityConfig;

namespace TasksWebApi.DataAccess;

public class TasksDbContext : IdentityDbContext<UserEntity>
{
    public TasksDbContext(DbContextOptions options) : base(options) { }
    public TasksDbContext() { }

    public DbSet<TaskEntity> Tasks { get; set; }
    public DbSet<TaskListEntity> TaskLists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TaskListEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TaskEntityConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
    }

    public override int SaveChanges()
    {
        UpdateSoftDeleteStatuses();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateSoftDeleteStatuses();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        UpdateSoftDeleteStatuses();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateSoftDeleteStatuses();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateSoftDeleteStatuses()
    {
        foreach (EntityEntry entry in ChangeTracker.Entries())
        {
            BaseEntity baseEntity = entry.Entity as BaseEntity;
            if (baseEntity == null)
                return;

            switch (entry.State)
            {
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    baseEntity.IsDeleted = true;
                    break;
            }
        }
    }
}