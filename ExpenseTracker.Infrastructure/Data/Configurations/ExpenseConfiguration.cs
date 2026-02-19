using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Data.Configurations;

public class ExpenseConfiguration: IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.HasKey(u => u.Id);
        builder.HasOne(e => e.User)
            .WithMany(u => u.Expenses)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Category)
            .WithMany()
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.OwnsOne(e => e.Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount");

            money.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3);
        });
    }
}