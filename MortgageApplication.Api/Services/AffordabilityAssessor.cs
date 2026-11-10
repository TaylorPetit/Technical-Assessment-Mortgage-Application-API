using MortgageApplicationApi.Models;

namespace MortgageApplicationApi.Services;

public class AffordabilityAssessor : IAffordabilityAssessor
{
    public const decimal AnnualInterestRate = 0.05m;
    public const decimal MaximumDebtToIncomeRatio = 0.40m;
    public const decimal MaximumLoanToValueRatio = 0.90m;

    public AffordabilityAssessment Assess(MortgageApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        long termMonths = (long)application.TermYears * 12;
        decimal requestedLoan = application.PropertyValue - application.DepositAmount;
        decimal monthlyGrossIncome = application.AnnualIncome / 12m;
        decimal monthlyPayment = CalculateMonthlyPayment(requestedLoan, termMonths);
        decimal loanToValue = requestedLoan / application.PropertyValue;
        decimal debtToIncome = (application.MonthlyExpenses + monthlyPayment) / monthlyGrossIncome;

        decimal availableMonthlyPayment = Math.Max(
            0m,
            monthlyGrossIncome * MaximumDebtToIncomeRatio - application.MonthlyExpenses);
        decimal incomeBasedMaximum = CalculateMaximumPrincipal(availableMonthlyPayment, termMonths);
        decimal loanToValueBasedMaximum = application.PropertyValue * MaximumLoanToValueRatio;
        decimal maximumBorrowing = Math.Min(incomeBasedMaximum, loanToValueBasedMaximum);

        bool approved = debtToIncome <= MaximumDebtToIncomeRatio && loanToValue <= MaximumLoanToValueRatio;
        string explanation = approved
            ? "The estimated payment is within the 40% debt-to-income limit and the loan is within the 90% loan-to-value limit."
            : debtToIncome > MaximumDebtToIncomeRatio
                ? "The estimated monthly payment and declared expenses exceed the 40% debt-to-income limit."
                : "The requested loan exceeds the 90% loan-to-value limit.";

        return new AffordabilityAssessment(
            application.Id,
            loanToValue,
            debtToIncome,
            approved,
            maximumBorrowing,
            explanation);
    }

    private static decimal CalculateMonthlyPayment(decimal principal, long termMonths)
    {
        decimal monthlyRate = AnnualInterestRate / 12m;
        decimal discountFactor = DecimalPower(1m / (1m + monthlyRate), termMonths);

        return principal * monthlyRate / (1m - discountFactor);
    }

    private static decimal CalculateMaximumPrincipal(decimal monthlyPayment, long termMonths)
    {
        if (monthlyPayment <= 0m)
        {
            return 0m;
        }

        decimal monthlyRate = AnnualInterestRate / 12m;
        decimal discountFactor = DecimalPower(1m / (1m + monthlyRate), termMonths);

        return monthlyPayment * (1m - discountFactor) / monthlyRate;
    }

    private static decimal DecimalPower(decimal value, long exponent)
    {
        decimal result = 1m;
        while (exponent > 0)
        {
            if ((exponent & 1) == 1)
            {
                result *= value;
            }

            exponent >>= 1;

            if (exponent > 0)
            {
                value *= value;
            }
        }

        return result;
    }
}
