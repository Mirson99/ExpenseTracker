namespace ExpenseTracker.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public string BaseCurrency { get; set; } = "PLN";
    public List<Expense> Expenses { get; set; }
}