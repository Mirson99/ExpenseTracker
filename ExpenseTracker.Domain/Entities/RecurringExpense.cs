using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Domain.Primitives;
using ExpenseTracker.Domain.ValueObjects;

namespace ExpenseTracker.Domain.Entities;

public class RecurringExpense: BaseEntity
{
    public Guid UserId { get; private set; }
    
    public string Name { get; private set; } = string.Empty;
    public Money Price { get; private set; }
    public int CategoryId { get; private set; }
    public Category Category { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Frequency Frequency { get; private set; }
    public DateTime NextExecutionDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    
    private RecurringExpense() { }
    
    public static RecurringExpense Create(string name, string? currency,decimal amount, DateTime date, int categoryId, Guid userId, string? description, Frequency? frequency)
    {
        var expense = new RecurringExpense
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CategoryId = categoryId,
            UserId = userId,
            Price = Money.From(amount, currency ?? "PLN"),
            NextExecutionDate = date,
            IsActive = true,
            Frequency = frequency ?? Frequency.Daily,
        };
        
        while (expense.NextExecutionDate <= DateTime.UtcNow.Date)
        {
            expense.MoveToNextDate();
        }
    
        return expense;
    }
    
    public void MoveToNextDate()
    {
        NextExecutionDate = Frequency switch
        {
            Frequency.Daily => NextExecutionDate.AddDays(1),
            Frequency.Weekly => NextExecutionDate.AddDays(7),
            Frequency.Monthly => NextExecutionDate.AddMonths(1),
            Frequency.Yearly => NextExecutionDate.AddYears(1),
            _ => NextExecutionDate.AddMonths(1)
        };
    }

    public void MakeRecurringExpenseInActive()
    {
        IsActive = false;
    }
}