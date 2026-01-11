using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using MediatR;

namespace ExpenseTracker.Application.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler: IRequestHandler<CreateCategoryCommand>
{
    private readonly IAppDbContext  _dbContext;
    private readonly ICurrentUserService _currentUser;
    public CreateCategoryCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }
    
    public async Task Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        await _dbContext.Categories.AddAsync(new Category()
        {
            Name = request.Name,
            IsSystem = false,
            UserId = _currentUser.UserId
        }, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}