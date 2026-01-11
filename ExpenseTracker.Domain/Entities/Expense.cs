namespace ExpenseTracker.Domain.Entities;

public class Expense
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PLN";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime Date {get; set;}
    public Category Category { get; set; }
    public User User { get; set; }
    public int CategoryId { get; set; }
    public Guid UserId { get; set; }
}