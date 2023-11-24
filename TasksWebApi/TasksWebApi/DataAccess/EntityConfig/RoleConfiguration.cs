using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TasksWebApi.Models;

namespace TasksWebApi.DataAccess.EntityConfig;

public sealed class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> entityBuilder)
    {
        entityBuilder.HasData(
            new IdentityRole
            {
                Name = Roles.SUPERADMIN, 
                NormalizedName = Roles.SUPERADMIN.ToUpperInvariant()
            });
    }
}