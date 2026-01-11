namespace ExpenseTracker.API.Requests;

public class CreateExpenseRequest
{
    public string Name { get; set; }
    public decimal Amount { get; set; }
    public int CategoryId { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public string? Currency { get; set; }
}