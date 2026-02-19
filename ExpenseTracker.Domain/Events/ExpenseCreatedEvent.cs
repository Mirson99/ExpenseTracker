using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Primitives;

namespace ExpenseTracker.Domain.Events;

public record ExpenseCreatedEvent(Expense Expense) : IDomainEvent;