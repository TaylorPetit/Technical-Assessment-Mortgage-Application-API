using WebApplication1.Models;
using WebApplication1.Services;

namespace MortgageApplication.Tests;

public sealed class AffordabilityAssessorTests
{
    private readonly AffordabilityAssessor _assessor = new();

    [Fact]
    public void Assess_ApprovesApplicationWithinDtiAndLtvLimits()
    {
        var application = CreateApplication(
            annualIncome: 100_000m,
            monthlyExpenses: 1_000m,
            depositAmount: 100_000m,
            propertyValue: 400_000m,
            termYears: 25);

        var result = _assessor.Assess(application);

        Assert.True(result.Approved);
        Assert.Equal(0.75m, result.LoanToValueRatio);
        Assert.InRange(result.DebtToIncomeRatio, 0m, AffordabilityAssessor.MaximumDebtToIncomeRatio);
        Assert.True(result.MaximumBorrowingEstimate > 0m);
        Assert.Contains("within", result.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Assess_DeclinesWhenLtvExceedsLimit()
    {
        var application = CreateApplication(
            annualIncome: 200_000m,
            monthlyExpenses: 500m,
            depositAmount: 10_000m,
            propertyValue: 400_000m,
            termYears: 25);

        var result = _assessor.Assess(application);

        Assert.False(result.Approved);
        Assert.Equal(0.975m, result.LoanToValueRatio);
        Assert.Contains("90% loan-to-value", result.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Assess_DeclinesWhenDtiExceedsLimit()
    {
        var application = CreateApplication(
            annualIncome: 60_000m,
            monthlyExpenses: 2_500m,
            depositAmount: 50_000m,
            propertyValue: 300_000m,
            termYears: 25);

        var result = _assessor.Assess(application);

        Assert.False(result.Approved);
        Assert.True(result.DebtToIncomeRatio > AffordabilityAssessor.MaximumDebtToIncomeRatio);
        Assert.Contains("debt-to-income", result.Explanation, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Assess_ReturnsZeroMaximumBorrowingWhenExpensesUseAllDtiCapacity()
    {
        var application = CreateApplication(
            annualIncome: 60_000m,
            monthlyExpenses: 2_000m,
            depositAmount: 100_000m,
            propertyValue: 300_000m,
            termYears: 25);

        var result = _assessor.Assess(application);

        Assert.Equal(0m, result.MaximumBorrowingEstimate);
    }

    private static MortgageApplication CreateApplication(
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
