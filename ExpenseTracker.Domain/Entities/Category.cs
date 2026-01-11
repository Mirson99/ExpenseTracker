namespace ExpenseTracker.Domain.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public Guid? UserId { get; set; }
    public bool IsSystem { get; set; }
}