using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Application.Expenses.Commands.ProcessRecurringExpenses;

public class ProcessRecurringExpensesCommandHandler : IRequestHandler<ProcessRecurringExpensesCommand>
{
    private readonly IAppDbContext _context;
    private readonly ILogger<ProcessRecurringExpensesCommandHandler> _logger;

    public ProcessRecurringExpensesCommandHandler(IAppDbContext context, ILogger<ProcessRecurringExpensesCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(ProcessRecurringExpensesCommand request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var expensesToProcess = await _context.RecurringExpenses
            .Where(x => x.IsActive && x.NextExecutionDate.Date <= today)
            .ToListAsync(cancellationToken);

        if (!expensesToProcess.Any()) return;

        foreach (var recurring in expensesToProcess)
        {
            var newExpense = new Expense
            {
                Id = Guid.NewGuid(),
                UserId = recurring.UserId,
                Name = recurring.Name,
                Amount = recurring.Amount,
                Currency = recurring.Currency,
                Date = today,
                CreatedAt = today,
                UpdatedAt = today,
                CategoryId = recurring.CategoryId
            };
            _context.Expenses.Add(newExpense);

            recurring.MoveToNextDate();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}