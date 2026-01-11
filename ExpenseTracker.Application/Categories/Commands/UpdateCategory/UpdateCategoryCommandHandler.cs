using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler: IRequestHandler<UpdateCategoryCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateCategoryCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }
    
    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
                          .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
                      ?? throw new NotFoundException($"Category with id {request.Id} not found");
        
        if (category.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException("User does not have permission to update category");

        category.Name = request.Name;

        await _context.SaveChangesAsync(cancellationToken);
    }
}