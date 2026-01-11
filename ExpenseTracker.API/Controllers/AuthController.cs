using ExpenseTracker.Application.Auth.Commands.Login;
using ExpenseTracker.Application.Auth.Commands.LoginWithRefreshToken;
using ExpenseTracker.Application.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("auth")]
public class AuthController: ControllerBase
{
    private readonly ISender  _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand request)
    { 
        var command = new RegisterCommand(request.Email, request.Password); 
        var result = await _sender.Send(command); 
        return Ok(new RegisterResponse {Id = result.Id});
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand request)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _sender.Send(command); 
        return Ok(result);
    }
    
    [HttpPost("refresh-token")]
    public async Task<IActionResult> Login(LoginWithRefreshTokenCommand request)
    {
        var result = await _sender.Send(request); 
        return Ok(result);
    }
}

public class RegisterResponse
{
    public Guid Id { get; set; }
}