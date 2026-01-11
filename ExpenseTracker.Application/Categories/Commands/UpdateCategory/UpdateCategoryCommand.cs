using MediatR;

namespace ExpenseTracker.Application.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(int Id, string Name): IRequest;