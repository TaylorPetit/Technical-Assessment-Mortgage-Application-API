namespace MortgageApplicationApi.Models;

public record MortgageApplication(
    Guid Id,
    decimal AnnualIncome,
    decimal MonthlyExpenses,
    decimal DepositAmount,
    decimal PropertyValue,
    int TermYears,
    DateTimeOffset CreatedAt);
