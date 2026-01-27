using ExpenseTracker.Application.Expenses.Commands.ProcessRecurringExpenses;
using MediatR;

namespace ExpenseTracker.API.BackgroundJobs;

public class RecurringExpenseJob
{
    private readonly ISender _sender;

    public RecurringExpenseJob(ISender sender)
    {
        _sender = sender;
    }
    
    public async Task Execute()
    {
        await _sender.Send(new ProcessRecurringExpensesCommand());
    }
}