using MediatR;

namespace ExpenseTracker.Application.Expenses.Commands.DeleteRecurringExpense;

public record DeleteRecurringExpenseCommand(Guid Id) : IRequest;