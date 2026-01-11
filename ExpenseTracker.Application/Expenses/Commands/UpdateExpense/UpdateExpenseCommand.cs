using MediatR;

namespace ExpenseTracker.Application.Expenses.Commands.UpdateExpense;

public sealed record UpdateExpenseCommand(Guid Id, string Name, decimal Amount, int CategoryId, DateTime Date, string? Description, string? Currency) : IRequest;