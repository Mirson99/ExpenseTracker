using ExpenseTracker.Domain.Entities;
using MediatR;

namespace ExpenseTracker.Application.Auth.Commands.Register;

public sealed record RegisterCommand(string Email, string Password): IRequest<User>;