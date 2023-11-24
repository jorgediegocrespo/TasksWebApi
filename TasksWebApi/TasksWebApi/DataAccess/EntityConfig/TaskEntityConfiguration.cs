using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TasksWebApi.DataAccess.Entities;

namespace TasksWebApi.DataAccess.EntityConfig;

public sealed class TaskEntityConfiguration : IEntityTypeConfiguration<TaskEntity>
{
    public void Configure(EntityTypeBuilder<TaskEntity> entityBuilder)
    {
        entityBuilder.ToTable("Tasks");

        entityBuilder.HasKey(x => x.Id);
        entityBuilder.Property(x => x.Id).ValueGeneratedOnAdd();

        entityBuilder.Property(x => x.Description).IsRequired();
        entityBuilder.HasOne(x => x.TaskList)
            .WithMany(x => x.Tasks)
            .IsRequired()
            .HasForeignKey(x => x.TaskListId)
            .IsRequired();
        
        entityBuilder.HasQueryFilter(x => !x.IsDeleted);
    }
}