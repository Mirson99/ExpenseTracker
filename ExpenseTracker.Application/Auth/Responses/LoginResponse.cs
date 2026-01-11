namespace ExpenseTracker.Application.Auth.Responses;

public class LoginResponse
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}