using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Data.Configurations;

public class CategoryConfiguration: IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.Name).IsUnique();
        
        builder.HasData(
            new Category { Id = 1, Name = "Food", IsSystem = true },
            new Category { Id = 2, Name = "Transport", IsSystem = true },
            new Category { Id = 3, Name = "Shopping", IsSystem = true },
            new Category { Id = 4, Name = "Entertainment", IsSystem = true },
            new Category { Id = 5, Name = "Healthcare", IsSystem = true },
            new Category { Id = 6, Name = "Bills", IsSystem = true },
            new Category { Id = 7, Name = "Housing", IsSystem = true },
            new Category { Id = 8, Name = "Education", IsSystem = true },
            new Category { Id = 9, Name = "Travel", IsSystem = true },
            new Category { Id = 10, Name = "Subscriptions", IsSystem = true },
            new Category { Id = 11, Name = "Savings", IsSystem = true },
            new Category { Id = 12, Name = "Other", IsSystem = true }
        );
    }
}