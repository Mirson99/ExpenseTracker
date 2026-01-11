using ExpenseTracker.Application.Common.Models;
using MediatR;

namespace ExpenseTracker.Application.Expenses.Queries.GetExpenseById;

public sealed record GetExpenseByIdQuery(Guid Id): IRequest<ExpenseResponse>;