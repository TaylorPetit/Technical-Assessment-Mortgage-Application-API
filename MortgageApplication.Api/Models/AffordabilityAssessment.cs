namespace MortgageApplicationApi.Models;

public record AffordabilityAssessment(
    Guid ApplicationId,
    decimal LoanToValueRatio,
    decimal DebtToIncomeRatio,
    bool Approved,
    decimal MaximumBorrowingEstimate,
    string Explanation);
