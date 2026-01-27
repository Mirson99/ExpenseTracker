using MediatR;

namespace ExpenseTracker.Application.Expenses.Commands.ProcessRecurringExpenses;

public record ProcessRecurringExpensesCommand : IRequest;