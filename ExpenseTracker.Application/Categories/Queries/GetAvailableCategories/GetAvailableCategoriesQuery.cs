using ExpenseTracker.Application.Common.Models;
using ExpenseTracker.Domain.Entities;
using MediatR;

namespace ExpenseTracker.Application.Categories.Queries.GetAvailableCategories;

public sealed record GetAvailableCategoriesQuery(): IRequest<IList<CategoryResponse>>;