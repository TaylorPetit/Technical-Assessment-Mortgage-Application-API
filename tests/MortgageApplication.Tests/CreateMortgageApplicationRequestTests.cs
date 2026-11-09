using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace MortgageApplication.Tests;

public sealed class CreateMortgageApplicationRequestTests
{
    [Fact]
    public void Validate_RejectsZeroAnnualIncome()
    {
        var request = CreateRequest(annualIncome: 0m);

        var errors = Validate(request);

        Assert.Contains(errors, error => error.MemberNames.Contains(nameof(request.AnnualIncome)));
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

    private static List<ValidationResult> Validate(CreateMortgageApplicationRequest request)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), errors, validateAllProperties: true);
        return errors;
    }

    private static CreateMortgageApplicationRequest CreateRequest(
        decimal annualIncome = 100_000m,
        decimal depositAmount = 100_000m) => new()
    {
        AnnualIncome = annualIncome,
        MonthlyExpenses = 1_200m,
        DepositAmount = depositAmount,
        PropertyValue = 400_000m,
        TermYears = 25
    };
}
