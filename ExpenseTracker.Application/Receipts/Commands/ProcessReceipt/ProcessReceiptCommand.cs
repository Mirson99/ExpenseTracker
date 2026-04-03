using MediatR;

namespace ExpenseTracker.Application.Receipts.Commands.ProcessReceipt;

public record ProcessReceiptCommand(string StorageKey) : IRequest<Guid>;