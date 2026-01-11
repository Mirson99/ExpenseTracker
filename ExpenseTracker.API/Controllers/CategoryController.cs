using ExpenseTracker.Application.Categories.Commands;
using ExpenseTracker.Application.Categories.Commands.DeleteCategory;
using ExpenseTracker.Application.Categories.Commands.UpdateCategory;
using ExpenseTracker.Application.Categories.Queries.GetAvailableCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

[ApiController]
[Route("category")]
public class CategoryController: ControllerBase
{
    private readonly ISender  _sender;

    public CategoryController(ISender sender)
    {
        _sender = sender;
    }
    
    [Authorize]
    [HttpPost("")]
    public async Task<IActionResult> Create(CreateCategoryCommand request)
    { 
        var command = new CreateCategoryCommand(request.Name);
        await _sender.Send(command);
        return NoContent();
    }
    
    [Authorize]
    [HttpGet("")]
    public async Task<IActionResult> GetAllAvailableCategories()
    {
        var query = new GetAvailableCategoriesQuery();
        var list = await _sender.Send(query);
        return Ok(list);
    }
    
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategoryById(int id)
    { 
        var command = new DeleteCategoryCommand(id);
        await _sender.Send(command);
        return NoContent();
    }
    
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryCommand request)
    {
        var command = new UpdateCategoryCommand(id, request.Name);
        await _sender.Send(command);
        return NoContent();
    }
}