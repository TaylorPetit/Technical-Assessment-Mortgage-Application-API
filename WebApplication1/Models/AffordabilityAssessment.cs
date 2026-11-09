namespace WebApplication1.Models;

public sealed record AffordabilityAssessment(
    Guid ApplicationId,
    decimal LoanToValueRatio,
    decimal DebtToIncomeRatio,
    bool Approved,
    decimal MaximumBorrowingEstimate,
    string Explanation);
