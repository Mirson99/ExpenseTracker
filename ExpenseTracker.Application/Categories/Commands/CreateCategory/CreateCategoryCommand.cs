using MediatR;

namespace ExpenseTracker.Application.Categories.Commands;

public sealed record CreateCategoryCommand(string Name): IRequest;