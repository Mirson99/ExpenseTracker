using ExpenseTracker.Domain.Enums;

namespace ExpenseTracker.Domain.Entities;

public class RecurringExpense
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PLN";
    public int CategoryId { get; set; }
    public Category Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public Frequency Frequency { get; set; }
    public DateTime NextExecutionDate { get; set; }
    public bool IsActive { get; set; } = true;
    
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
}