using ExpenseTracker.Domain.Events;
using ExpenseTracker.Domain.Primitives;
using ExpenseTracker.Domain.ValueObjects;

namespace ExpenseTracker.Domain.Entities;

public enum ExpenseStatus
{
    Confirmed = 0,             
    Processing = 1,            
    RequiresVerification = 2,  
    Failed = 3                
}

public class Expense: BaseEntity
{
    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public Money? Price { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? Date {get; private set;}
    public Category? Category { get; private set; }
    public User User { get; private set; }
    public int? CategoryId { get; private set; }
    public Guid UserId { get; private set; }
    public bool IsRecurring { get; private set; }
    public ExpenseStatus Status { get; private set; }
    public string? StorageKey { get; private set; }
    private Expense() { }
    
    public static Expense CreateManual(string name, string? currency,decimal? amount, DateTime date, int categoryId, bool isRecurring, Guid userId, string? description)
    {
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Date = date,
            CategoryId = categoryId,
            UserId = userId,
            Price = Money.From(amount ?? 0, currency ?? "PLN"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsRecurring = isRecurring,
            Status = ExpenseStatus.Confirmed,
        };
        
        expense.AddDomainEvent(new ExpenseCreatedEvent(expense));

        return expense;
    }
    
    public static Expense CreateFromReceipt(Guid userId, string storageKey)
    {
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StorageKey = storageKey,
            Status = ExpenseStatus.Processing,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsRecurring = false
        };
        expense.AddDomainEvent(new ReceiptProcessingStartedEvent(expense));
        return expense;
    }
    
 
    //
    // // 4. NOWA METODA - Wywoływana z frontendu, gdy user klika "Zatwierdź" (Faza 6.4)
    // public void ConfirmReceiptData(string finalName, Money finalPrice, DateTime finalDate, int finalCategoryId, string? description)
    // {
    //     if (Status != ExpenseStatus.RequiresVerification)
    //         throw new InvalidOperationException("Expense is not pending verification.");
    //
    //     Name = finalName;
    //     Price = finalPrice;
    //     Date = finalDate;
    //     CategoryId = finalCategoryId;
    //     Description = description;
    //     Status = ExpenseStatus.Confirmed;
    //     UpdatedAt = DateTime.UtcNow;
    //     
    //     // Dopiero teraz, gdy wydatek jest pełnoprawny, możemy wyemitować główny event
    //     AddDomainEvent(new ExpenseCreatedEvent(this));
    // }
    //
    // // 5. Oznaczanie błędu (gdy Polly wyczerpie Retry)
    // public void MarkAsFailed()
    // {
    //     Status = ExpenseStatus.Failed;
    //     UpdatedAt = DateTime.UtcNow;
    // }
    
    public void ApplyAiRecognition(decimal recognizedAmount, string currency, DateTime recognizedDate, string recognizedMerchant, string description, int categoryId)
    {
        if (Status != ExpenseStatus.Processing)
            throw new InvalidOperationException("Only processing expenses can be updated by AI.");

        Name = recognizedMerchant;
        Price = Money.From(recognizedAmount, currency);
        Date = recognizedDate;
        Status = ExpenseStatus.RequiresVerification;
        UpdatedAt = DateTime.UtcNow;
        Description = description;
        CategoryId = categoryId;
    }

    public void ConfirmReceiptData(string finalName, Money finalPrice, DateTime finalDate, int finalCategoryId, string? description)
    {
        if (Status != ExpenseStatus.RequiresVerification)
            throw new InvalidOperationException("Expense is not pending verification.");

        Name = finalName;
        Price = finalPrice;
        Date = finalDate;
        CategoryId = finalCategoryId;
        Description = description;
        Status = ExpenseStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ExpenseCreatedEvent(this));
    }

    public void MarkAsFailed()
    {
        Status = ExpenseStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string name, Money price, DateTime date, int categoryId, string? description, ExpenseStatus? status)
    {
        Name = name;
        Price = price;
        Date = date;
        CategoryId = categoryId;
        Description = description ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
        Status = status ?? ExpenseStatus.Confirmed;
    }
}