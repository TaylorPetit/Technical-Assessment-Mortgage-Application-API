using WebApplication1.Models;

namespace WebApplication1.Services;

/// <summary>
/// Provides a deliberately simplified affordability estimate for the exercise.
/// </summary>
public sealed class AffordabilityAssessor
{
    public const decimal AnnualInterestRate = 0.05m;
    public const decimal MaximumDebtToIncomeRatio = 0.40m;
    public const decimal MaximumLoanToValueRatio = 0.90m;

    public AffordabilityAssessment Assess(MortgageApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        var requestedLoan = application.PropertyValue - application.DepositAmount;
        var monthlyGrossIncome = application.AnnualIncome / 12m;
        var monthlyPayment = CalculateMonthlyPayment(requestedLoan, application.TermYears);
        var ltv = requestedLoan / application.PropertyValue;
        var dti = (application.MonthlyExpenses + monthlyPayment) / monthlyGrossIncome;

        var availableMonthlyPayment = Math.Max(
            0m,
            monthlyGrossIncome * MaximumDebtToIncomeRatio - application.MonthlyExpenses);
        var incomeBasedMaximum = CalculateMaximumPrincipal(availableMonthlyPayment, application.TermYears);
        var ltvBasedMaximum = application.PropertyValue * MaximumLoanToValueRatio;
        var maximumBorrowing = Math.Min(incomeBasedMaximum, ltvBasedMaximum);

        var approved = dti <= MaximumDebtToIncomeRatio && ltv <= MaximumLoanToValueRatio;
        var explanation = approved
            ? "The estimated payment is within the 40% debt-to-income limit and the loan is within the 90% loan-to-value limit."
            : dti > MaximumDebtToIncomeRatio
                ? "The estimated monthly payment and declared expenses exceed the 40% debt-to-income limit."
                : "The requested loan exceeds the 90% loan-to-value limit.";

        return new AffordabilityAssessment(
            application.Id,
            ltv,
            dti,
            approved,
            maximumBorrowing,
            explanation);
    }

    private static decimal CalculateMonthlyPayment(decimal principal, int termYears)
    {
        var monthlyRate = AnnualInterestRate / 12m;
        var periods = termYears * 12;
        var growthFactor = DecimalPower(1m + monthlyRate, periods);

        return principal * monthlyRate * growthFactor / (growthFactor - 1m);
    }

    private static decimal CalculateMaximumPrincipal(decimal monthlyPayment, int termYears)
    {
        if (monthlyPayment <= 0m)
        {
            return 0m;
        }

        var monthlyRate = AnnualInterestRate / 12m;
        var periods = termYears * 12;
        var discountFactor = DecimalPower(1m + monthlyRate, periods);

        return monthlyPayment * (discountFactor - 1m) / (monthlyRate * discountFactor);
    }

    private static decimal DecimalPower(decimal value, int exponent)
    {
        var result = 1m;
        for (var i = 0; i < exponent; i++)
        {
            result *= value;
        }

        return result;
    }
}
