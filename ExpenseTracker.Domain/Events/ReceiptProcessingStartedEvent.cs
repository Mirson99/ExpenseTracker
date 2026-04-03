using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Primitives;
using MediatR;

namespace ExpenseTracker.Domain.Events;

public record ReceiptProcessingStartedEvent(Expense Expense) : IDomainEvent;