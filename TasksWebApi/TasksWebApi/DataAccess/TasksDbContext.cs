using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.DataAccess.EntityConfig;
using TasksWebApi.Services;

namespace TasksWebApi.DataAccess;

public class TasksDbContext : IdentityDbContext<UserEntity>
{
    private readonly IHttpContextService _httpContextService;
    public TasksDbContext(IHttpContextService httpContextService, DbContextOptions<TasksDbContext> options) : base(options)
    {
        _httpContextService = httpContextService;
    }
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
        UpdateBaseEntityInfo();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateBaseEntityInfo();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        UpdateBaseEntityInfo();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateBaseEntityInfo();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    public Task<int> SaveChangesWithoutSoftDeleteAsync(CancellationToken cancellationToken = default)
    {
        UpdateBaseEntityInfo(false);
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateBaseEntityInfo(bool softDelete = true)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            var baseEntity = entry.Entity as BaseEntity;
            if (baseEntity == null)
                return;

            switch (entry.State)
            {
                case EntityState.Deleted:
                    if (softDelete)
                    {
                        entry.State = EntityState.Modified;
                        baseEntity.IsDeleted = true;
                        baseEntity.DeleteUser = _httpContextService.GetContextUser().Id;
                        baseEntity.DateOfDelete = DateTime.UtcNow;
                    }
                    break;
                case EntityState.Added:
                    baseEntity.CreationUser = _httpContextService.GetContextUser().Id;
                    baseEntity.DateOfCreation = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    baseEntity.LastUpdateUser = _httpContextService.GetContextUser().Id;
                    baseEntity.DateOfLastUpdate = DateTime.UtcNow;
                    break;
            }
        }
    }
}