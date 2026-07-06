using AjoCoreBackend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjoCoreBackend.Persistence.Configurations
{
    public class SystemAdminConfiguration : IEntityTypeConfiguration<SystemAdmin>
    {
        public void Configure(EntityTypeBuilder<SystemAdmin> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.Username).IsUnique();
            
            builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
            builder.Property(x => x.Username).IsRequired().HasMaxLength(100);
            builder.Property(x => x.PasswordHash).IsRequired();
            builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);

            // Seed System Admins
            builder.HasData(
                new SystemAdmin
                {
                    Id = System.Guid.Parse("d8a8b132-23cf-4235-866c-333e213320c4"),
                    FirstName = "Oriolowo",
                    LastName = "Mustapha",
                    Username = "apha",
                    Email = "baseapha@gmail.com",
                    PasswordHash = "$2a$12$BjORNSCRSBhTUck456hyBuQz4dhMQMj/HcDnmOUxWiGMRARp9Qln6",
                    Role = Domain.Enum.UserRole.SystemAdmin,
                    CreatedAt = new System.DateTime(2026, 1, 1, 0, 0, 0, System.DateTimeKind.Utc),
                    UpdatedAt = new System.DateTime(2026, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)
                },
                new SystemAdmin
                {
                    Id = System.Guid.Parse("f827e8d6-444f-4a0b-8d14-b5ebc5344d57"),
                    FirstName = "Adeyemi",
                    LastName = "Mubarak",
                    Username = "ghost",
                    Email = "adeyemiadigun12@gmail.com",
                    PasswordHash = "$2a$12$q28FxVwEjqbz66XrZ8NfVu56dS8ZxAUPrVHRAwPLu4.Nakr/bJwm6",
                    Role = Domain.Enum.UserRole.SystemAdmin,
                    CreatedAt = new System.DateTime(2026, 1, 1, 0, 0, 0, System.DateTimeKind.Utc),
                    UpdatedAt = new System.DateTime(2026, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)
                }
            );
        }
    }
}
