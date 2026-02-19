using ExpenseTracker.Domain.Events;
using ExpenseTracker.Domain.Primitives;
using ExpenseTracker.Domain.ValueObjects;

namespace ExpenseTracker.Domain.Entities;

public class Expense: BaseEntity
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Money Price { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime Date {get; private set;}
    public Category Category { get; private set; }
    public User User { get; private set; }
    public int CategoryId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsRecurring { get; private set; }
    private Expense() { }
    
    public static Expense Create(string name, string? currency,decimal amount, DateTime date, int categoryId, bool isRecurring, Guid userId, string? description)
    {
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Date = date,
            CategoryId = categoryId,
            UserId = userId,
            Price = Money.From(amount, currency ?? "PLN"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsRecurring = isRecurring,
        };
        
        expense.AddDomainEvent(new ExpenseCreatedEvent(expense));

        return expense;
    }
    
    public void Update(string name, Money price, DateTime date, int categoryId, string? description)
    {
        Name = name;
        Price = price;
        Date = date;
        CategoryId = categoryId;
        Description = description ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }
}