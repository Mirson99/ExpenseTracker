using ExpenseTracker.Application.Common.Models;
using ExpenseTracker.Domain.Entities;
using MediatR;

namespace ExpenseTracker.Application.Expenses.Queries;

public sealed record GetUserExpensesQuery(string? SearchTerm, string? SortColumn, string? SortOrder, int? CategoryId, int Page, int PageSize): IRequest<PagedList<ExpenseResponse>>;