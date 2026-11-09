namespace MortgageApplication.Web.Models;

public record MortgageApplicationResponse(
    Guid Id,
    decimal AnnualIncome,
    decimal MonthlyExpenses,
    decimal DepositAmount,
    decimal PropertyValue,
    int TermYears,
    DateTimeOffset CreatedAt);
