using ExpenseTracker.Application.Common.Models;
using ExpenseTracker.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Categories.Queries.GetAvailableCategories;

public class GetAvailableCategoriesQueryHandler: IRequestHandler<GetAvailableCategoriesQuery, IList<CategoryResponse>>
{
    private readonly IAppDbContext  _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAvailableCategoriesQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }
    
    public async Task<IList<CategoryResponse>> Handle(GetAvailableCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Categories.Where(c => c.UserId == _currentUserService.UserId || c.IsSystem == true).Select(c => new CategoryResponse()
        {
            Name = c.Name,
            Id = c.Id
        }).ToListAsync();
    }
}