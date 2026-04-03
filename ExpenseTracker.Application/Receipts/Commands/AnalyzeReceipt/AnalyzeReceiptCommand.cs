using MediatR;

namespace ExpenseTracker.Application.Receipts.Commands.AnalyzeReceipt;

public record AnalyzeReceiptCommand(Guid ExpenseId, string StorageKey) : IRequest<bool>;