using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Data.Configurations;

public class RecurringExpenseConfiguration: IEntityTypeConfiguration<RecurringExpense>
{
    public void Configure(EntityTypeBuilder<RecurringExpense> builder)
    {
        
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasOne(x => x.Category)     
            .WithMany()                      
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); 
    }
}