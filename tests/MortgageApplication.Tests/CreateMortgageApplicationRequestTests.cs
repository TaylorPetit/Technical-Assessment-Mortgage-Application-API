using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using MortgageApplicationApi.Models;

namespace MortgageApplication.Tests;

public class CreateMortgageApplicationRequestTests
{
    [Fact]
    public void Validate_RejectsZeroAnnualIncome()
    {
        var request = CreateRequest(annualIncome: 0m);

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.AnnualIncome)));
    }

    [Fact]
    public void Validate_RejectsNegativeMonthlyExpenses()
    {
        var request = CreateRequest(monthlyExpenses: -1m);

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.MonthlyExpenses)));
    }

    [Fact]
    public void Validate_RejectsNegativeDeposit()
    {
        var request = CreateRequest(depositAmount: -1m);

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.DepositAmount)));
    }

    [Fact]
    public void Validate_RejectsZeroPropertyValue()
    {
        var request = CreateRequest(propertyValue: 0m);

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.PropertyValue)));
    }

    [Fact]
    public void Validate_RejectsZeroTerm()
    {
        var request = CreateRequest(termYears: 0);

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.TermYears)));
    }

    [Fact]
    public void Deserialize_RejectsMissingRequiredFields()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<CreateMortgageApplicationRequest>("{}"));
    }

    [Fact]
    public void Validate_AcceptsDepositEqualToPropertyValue()
    {
        var request = CreateRequest(depositAmount: 400_000m);

        var errors = Validate(request);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_RejectsDepositGreaterThanPropertyValue()
    {
        var request = CreateRequest(depositAmount: 400_001m);

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.DepositAmount)));
    }

    [Fact]
    public void Validate_AcceptsValidApplication()
    {
        var errors = Validate(CreateRequest());

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_AcceptsAnnualIncomeAboveOneTrillion()
    {
        var errors = Validate(CreateRequest(annualIncome: 1_000_000_000_001m));

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_AcceptsDecimalMaximumAmountsAndLargestTerm()
    {
        var request = CreateRequest(
            annualIncome: decimal.MaxValue,
            monthlyExpenses: decimal.MaxValue,
            depositAmount: 0m,
            propertyValue: decimal.MaxValue,
            termYears: int.MaxValue);

        var errors = Validate(request);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_AcceptsMinimumPositiveIncomePropertyValueAndTerm()
    {
        var request = CreateRequest(
            annualIncome: 0.01m,
            monthlyExpenses: 0m,
            depositAmount: 0m,
            propertyValue: 0.01m,
            termYears: 1);

        var errors = Validate(request);

        Assert.Empty(errors);
    }

    private static List<ValidationResult> Validate(CreateMortgageApplicationRequest request)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), errors, validateAllProperties: true);
        return errors;
    }

    private static CreateMortgageApplicationRequest CreateRequest(
        decimal annualIncome = 100_000m,
        decimal monthlyExpenses = 1_200m,
        decimal depositAmount = 100_000m,
        decimal propertyValue = 400_000m,
        int termYears = 25) => new()
    {
        AnnualIncome = annualIncome,
        MonthlyExpenses = monthlyExpenses,
        DepositAmount = depositAmount,
        PropertyValue = propertyValue,
        TermYears = termYears
    };
}
