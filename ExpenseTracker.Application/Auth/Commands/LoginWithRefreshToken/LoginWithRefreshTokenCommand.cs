using ExpenseTracker.Application.Auth.Responses;
using MediatR;

namespace ExpenseTracker.Application.Auth.Commands.LoginWithRefreshToken;

public sealed record LoginWithRefreshTokenCommand(string RefreshToken) : IRequest<LoginResponse>;