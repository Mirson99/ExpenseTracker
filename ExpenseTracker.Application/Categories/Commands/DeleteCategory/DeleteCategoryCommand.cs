using MediatR;

namespace ExpenseTracker.Application.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(int Id): IRequest;