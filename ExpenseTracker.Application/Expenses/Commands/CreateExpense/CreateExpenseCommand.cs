using MediatR;

namespace ExpenseTracker.Application.Expenses.Commands;

public sealed record CreateExpenseCommand(string Name, decimal Amount, int CategoryId, DateTime Date, string? Description, string? Currency) : IRequest<Guid>;