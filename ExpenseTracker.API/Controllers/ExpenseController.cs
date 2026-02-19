using ExpenseTracker.API.Requests;
using ExpenseTracker.Application.Auth.Commands.Register;
using ExpenseTracker.Application.Expenses.Commands;
using ExpenseTracker.Application.Expenses.Commands.DeleteExpense;
using ExpenseTracker.Application.Expenses.Commands.DeleteRecurringExpense;
using ExpenseTracker.Application.Expenses.Commands.UpdateExpense;
using ExpenseTracker.Application.Expenses.Queries;
using ExpenseTracker.Application.Expenses.Queries.GetExpenseById;
using ExpenseTracker.Application.Expenses.Queries.GetUserRecurringExpenses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("expense")]
public class ExpenseController: ControllerBase
{
    private readonly ISender  _sender;

    public ExpenseController(ISender sender)
    {
        _sender = sender;
    }

    [Authorize]
    [HttpPost("")]
    public async Task<IActionResult> Create(CreateExpenseRequest request)
    { 
        var command = new CreateExpenseCommand(request.Name, request.Amount, request.CategoryId, request.Date, request.Description, request.Currency, request.IsRecurring, request.Frequency);
        var id = await _sender.Send(command);
        return Ok(id);
    }
    
    [Authorize]
    [HttpGet("")]
    public async Task<IActionResult> GetAllUserExpenses(string? searchTerm, string? sortColumn, string? sortOrder, int? categoryId, int page = 1, int pageSize = 10)
    { 
        var query = new GetUserExpensesQuery(searchTerm, sortColumn, sortOrder, categoryId, page, pageSize);
        var list = await _sender.Send(query);
        return Ok(list);
    }
    
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetExpenseById(Guid id)
    { 
        var query = new GetExpenseByIdQuery(id);
        var list = await _sender.Send(query);
        return Ok(list);
    }
    
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpenseById(Guid id)
    { 
        var command = new DeleteExpenseCommand(id);
        await _sender.Send(command);
        return NoContent();
    }
    
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExpenseCommand request)
    {
        var command = new UpdateExpenseCommand(id, request.Name, request.Amount, request.CategoryId, request.Date, request.Description, request.Currency);
        await _sender.Send(command);
        return NoContent();
    }
    
    [Authorize]
    [HttpDelete("recurring/{id}")]
    public async Task<IActionResult> DeleteRecurringExpenseById(Guid id)
    { 
        var command = new DeleteRecurringExpenseCommand(id);
        await _sender.Send(command);
        return NoContent();
    }
    
    [Authorize]
    [HttpGet("recurring")]
    public async Task<IActionResult> GetUserRecurringExpenses(string? searchTerm, string? sortColumn, string? sortOrder, int? categoryId, int page = 1, int pageSize = 10)
    { 
        var query = new GetUserRecurringExpensesQuery(searchTerm, sortColumn, sortOrder, categoryId, page, pageSize);
        var list = await _sender.Send(query);
        return Ok(list);
    }
}