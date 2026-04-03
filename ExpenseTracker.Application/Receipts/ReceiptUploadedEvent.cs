namespace ExpenseTracker.Application.Receipts;

public record ReceiptUploadedEvent(Guid ExpenseId, string StorageKey);