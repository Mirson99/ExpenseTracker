using MediatR;

namespace ExpenseTracker.Application.Expenses.Commands.DeleteExpense;

public sealed record DeleteExpenseCommand(Guid Id): IRequest;