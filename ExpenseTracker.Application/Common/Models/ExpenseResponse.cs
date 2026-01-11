namespace ExpenseTracker.Application.Common.Models;

public class ExpenseResponse
{
    public string Name { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public string CategoryName { get; set; }
    public string Currency { get; set; }
}