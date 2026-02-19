using ExpenseTracker.Application.Common.Models;
using MediatR;

namespace ExpenseTracker.Application.Expenses.Queries.GetUserRecurringExpenses;

public record GetUserRecurringExpensesQuery(string? SearchTerm, string? SortColumn, string? SortOrder, int? CategoryId, int Page, int PageSize): IRequest<PagedList<RecurringExpenseResponse>>;