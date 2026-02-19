namespace ExpenseTracker.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }
    
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
    
    public static Money From(decimal amount, string currency)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than 0");
        if  (string.IsNullOrEmpty(currency))
            throw new ArgumentException("Currency cannot be null or empty");
        return new Money(amount, currency.ToUpper());
    }
    
    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Currency must be equal");
        return new Money(left.Amount + right.Amount, left.Currency);
    }
}