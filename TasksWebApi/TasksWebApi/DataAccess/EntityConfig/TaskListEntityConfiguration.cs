using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TasksWebApi.DataAccess.Entities;

namespace TasksWebApi.DataAccess.EntityConfig;

public sealed class TaskListEntityConfiguration : IEntityTypeConfiguration<TaskListEntity>
{
    public void Configure(EntityTypeBuilder<TaskListEntity> entityBuilder)
    {
        entityBuilder.ToTable("TaskLists");

        entityBuilder.HasKey(x => x.Id);
        entityBuilder.Property(x => x.Id).ValueGeneratedOnAdd();

        entityBuilder.Property(x => x.Name).IsRequired();
        entityBuilder.HasMany(x => x.Tasks).WithOne(x => x.TaskList);
        entityBuilder.HasIndex(a => a.Name).IsUnique();
        
        entityBuilder.HasQueryFilter(x => !x.IsDeleted);
    }
}