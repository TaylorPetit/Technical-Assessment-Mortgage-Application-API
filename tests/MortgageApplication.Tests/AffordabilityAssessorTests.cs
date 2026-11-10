using MortgageApplicationApi.Models;
using MortgageApplicationApi.Services;
using MortgageApplicationModel = MortgageApplicationApi.Models.MortgageApplication;

namespace MortgageApplication.Tests;

public class AffordabilityAssessorTests
{
    private readonly AffordabilityAssessor _assessor = new();

    [Fact]
    public void Assess_ApprovesApplicationWithinDtiAndLtvLimits()
    {
        MortgageApplicationModel application = CreateApplication(
            annualIncome: 100_000m,
            monthlyExpenses: 1_000m,
            depositAmount: 100_000m,
            propertyValue: 400_000m,
            termYears: 25);

        AffordabilityAssessment result = _assessor.Assess(application);

        Assert.True(result.Approved);
        Assert.Equal(0.75m, result.LoanToValueRatio);
        Assert.InRange(result.DebtToIncomeRatio, 0m, AffordabilityAssessor.MaximumDebtToIncomeRatio);
        Assert.True(result.MaximumBorrowingEstimate > 0m);
        Assert.Contains("within", result.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Assess_CalculatesPaymentAndIncomeBasedMaximumBorrowing()
    {
        MortgageApplicationModel application = CreateApplication(
            annualIncome: 100_000m,
            monthlyExpenses: 1_200m,
            depositAmount: 700_000m,
            propertyValue: 1_000_000m,
            termYears: 25);

        AffordabilityAssessment result = _assessor.Assess(application);

        Assert.InRange(result.DebtToIncomeRatio, 0.3544524148m, 0.3544524151m);
        Assert.InRange(result.MaximumBorrowingEstimate, 364_928.09m, 364_928.11m);
    }

    [Fact]
    public void Assess_ApprovesApplicationAtDtiLimit()
    {
        MortgageApplicationModel application = CreateApplication(
            annualIncome: 120_000m,
            monthlyExpenses: 4_000m,
            depositAmount: 400_000m,
            propertyValue: 400_000m,
            termYears: 25);

        AffordabilityAssessment result = _assessor.Assess(application);

        Assert.Equal(AffordabilityAssessor.MaximumDebtToIncomeRatio, result.DebtToIncomeRatio);
        Assert.True(result.Approved);
    }

    [Fact]
    public void Assess_ApprovesApplicationAtLtvLimitAndCapsMaximumBorrowing()
    {
        MortgageApplicationModel application = CreateApplication(
            annualIncome: 1_000_000m,
            monthlyExpenses: 0m,
            depositAmount: 40_000m,
            propertyValue: 400_000m,
            termYears: 25);

        AffordabilityAssessment result = _assessor.Assess(application);

        Assert.Equal(AffordabilityAssessor.MaximumLoanToValueRatio, result.LoanToValueRatio);
        Assert.True(result.Approved);
        Assert.Equal(360_000m, result.MaximumBorrowingEstimate);
    }

    [Fact]
    public void Assess_HandlesLargestSupportedTermWithoutIteratingOncePerMonth()
    {
        MortgageApplicationModel application = CreateApplication(
            annualIncome: 100_000m,
            monthlyExpenses: 0m,
            depositAmount: 1_000m,
            propertyValue: 2_000m,
            termYears: int.MaxValue);

        AffordabilityAssessment result = _assessor.Assess(application);

        Assert.InRange(result.DebtToIncomeRatio, 0.000499999m, 0.000500001m);
        Assert.Equal(1_800m, result.MaximumBorrowingEstimate);
    }

    [Fact]
    public void Assess_DeclinesWhenLtvExceedsLimit()
    {
        MortgageApplicationModel application = CreateApplication(
            annualIncome: 200_000m,
            monthlyExpenses: 500m,
            depositAmount: 10_000m,
            propertyValue: 400_000m,
            termYears: 25);

        AffordabilityAssessment result = _assessor.Assess(application);

        Assert.False(result.Approved);
        Assert.Equal(0.975m, result.LoanToValueRatio);
        Assert.Contains("90% loan-to-value", result.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Assess_DeclinesWhenDtiExceedsLimit()
    {
        MortgageApplicationModel application = CreateApplication(
            annualIncome: 60_000m,
            monthlyExpenses: 2_500m,
            depositAmount: 50_000m,
            propertyValue: 300_000m,
            termYears: 25);

        AffordabilityAssessment result = _assessor.Assess(application);

        Assert.False(result.Approved);
        Assert.True(result.DebtToIncomeRatio > AffordabilityAssessor.MaximumDebtToIncomeRatio);
        Assert.Contains("debt-to-income", result.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Assess_ReturnsZeroMaximumBorrowingWhenExpensesUseAllDtiCapacity()
    {
        MortgageApplicationModel application = CreateApplication(
            annualIncome: 60_000m,
            monthlyExpenses: 2_000m,
            depositAmount: 100_000m,
            propertyValue: 300_000m,
            termYears: 25);

        AffordabilityAssessment result = _assessor.Assess(application);

        Assert.Equal(0m, result.MaximumBorrowingEstimate);
    }

    private static MortgageApplicationModel CreateApplication(
        decimal annualIncome,
        decimal monthlyExpenses,
        decimal depositAmount,
        decimal propertyValue,
        int termYears) => new(
            Guid.NewGuid(),
            annualIncome,
            monthlyExpenses,
            depositAmount,
            propertyValue,
            termYears,
            DateTimeOffset.UtcNow);
}
