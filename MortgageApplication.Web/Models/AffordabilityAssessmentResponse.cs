namespace MortgageApplication.Web.Models;

public record AffordabilityAssessmentResponse(
    Guid ApplicationId,
    decimal LoanToValueRatio,
    decimal DebtToIncomeRatio,
    bool Approved,
    decimal MaximumBorrowingEstimate,
    string Explanation);
