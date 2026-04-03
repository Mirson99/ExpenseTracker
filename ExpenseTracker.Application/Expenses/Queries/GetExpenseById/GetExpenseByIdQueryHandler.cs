using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Models;
using ExpenseTracker.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Expenses.Queries.GetExpenseById;

public class GetExpenseByIdQueryHandler: IRequestHandler<GetExpenseByIdQuery, ExpenseResponse>
{
    private readonly IAppDbContext  _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public GetExpenseByIdQueryHandler(IAppDbContext context, ICurrentUserService currentUserService,  IFileStorageService fileStorageService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }
    
    public async Task<ExpenseResponse> Handle(GetExpenseByIdQuery request, CancellationToken cancellationToken)
    {
        var expense = await _context.Expenses.Include(e => e.Category)
                          .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
                      ?? throw new NotFoundException($"Expense with id {request.Id} not found");
        
        if (expense.UserId != _currentUserService.UserId)
            throw new ForbiddenAccessException("User does not have permission to update expense");

        string? receiptUrl = null;
        if (!string.IsNullOrEmpty(expense.StorageKey))
        {
            receiptUrl = await _fileStorageService.DownloadFileAsync(
                expense.StorageKey,
                TimeSpan.FromMinutes(15));
        }

        return new ExpenseResponse()
        {
            Name = expense.Name,
            Description = expense.Description ?? "",
            Currency = expense.Price.Currency,
            Amount = expense.Price.Amount,
            Date = expense.Date,
            CategoryName = expense.Category.Name,
            ReceiptUrl = receiptUrl,
            Status = expense.Status,
        };
    }
}