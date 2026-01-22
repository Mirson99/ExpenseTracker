using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Application.Auth.Commands.Register;

public class RegisterCommandHandler: IRequestHandler<RegisterCommand, User>
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RegisterCommandHandler> _logger;
    
    public RegisterCommandHandler(IPasswordHasher passwordHasher, IUserRepository userRepository, ILogger<RegisterCommandHandler> logger)
    {
        _passwordHasher = passwordHasher;
        _userRepository = userRepository;
        _logger = logger;
    }
    
    public async Task<User> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            throw new ValidationException("User already exists");
        }
        var user = new User
        {
            Email = request.Email.ToLower(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
        };

        return await _userRepository.CreateAsync(user, cancellationToken);
    }
}