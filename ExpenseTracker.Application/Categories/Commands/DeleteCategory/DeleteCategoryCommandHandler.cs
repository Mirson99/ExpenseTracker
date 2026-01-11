using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler: IRequestHandler<DeleteCategoryCommand>
{
    private readonly IAppDbContext  _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCategoryCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }
    
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
                          .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
                      ?? throw new NotFoundException($"Expense with id {request.Id} not found");
        
        if (category.UserId != _currentUserService.UserId)
            throw new ForbiddenAccessException("User does not have permission to update expense");
        
        var hasExpenses = await _context.Expenses
            .AnyAsync(e => e.CategoryId == request.Id, cancellationToken);

        if (hasExpenses)
            throw new ValidationException("Cannot delete category with existing expenses");
        
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
    }
}