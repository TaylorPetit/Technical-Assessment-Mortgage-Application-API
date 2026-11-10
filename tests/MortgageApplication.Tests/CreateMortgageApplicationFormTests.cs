using System.ComponentModel.DataAnnotations;
using MortgageApplication.Web.Models;

namespace MortgageApplication.Tests;

public class CreateMortgageApplicationFormTests
{
    [Fact]
    public void Validate_RejectsBlankInputsIncludingZeroAllowedFields()
    {
        CreateMortgageApplicationRequest request = new();
        List<ValidationResult> results = new();

        bool isValid = Validator.TryValidateObject(request, new ValidationContext(request), results, true);

        Assert.False(isValid);
        string[] requiredFields =
        [
            nameof(request.AnnualIncome),
            nameof(request.MonthlyExpenses),
            nameof(request.DepositAmount),
            nameof(request.PropertyValue),
            nameof(request.TermYears)
        ];

        foreach (string field in requiredFields)
        {
            Assert.Contains(results, result => result.MemberNames.Contains(field));
        }
    }

    [Fact]
    public void Validate_DepositAbovePropertyValue_AttachesErrorToDepositField()
    {
        CreateMortgageApplicationRequest request = new()
        {
            AnnualIncome = 100_000m,
            MonthlyExpenses = 1_200m,
            DepositAmount = 500_000m,
            PropertyValue = 400_000m,
            TermYears = 25
        };
        List<ValidationResult> results = new();

        bool isValid = Validator.TryValidateObject(request, new ValidationContext(request), results, true);

        Assert.False(isValid);
        ValidationResult error = Assert.Single(results);
        Assert.Equal("Deposit cannot exceed the property value.", error.ErrorMessage);
        Assert.Equal([nameof(request.DepositAmount)], error.MemberNames);
    }

    [Fact]
    public void Validate_DepositWithinPropertyValue_ReturnsValid()
    {
        CreateMortgageApplicationRequest request = new()
        {
            AnnualIncome = 100_000m,
            MonthlyExpenses = 1_200m,
            DepositAmount = 100_000m,
            PropertyValue = 400_000m,
            TermYears = 25
        };
        List<ValidationResult> results = new();

        bool isValid = Validator.TryValidateObject(request, new ValidationContext(request), results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void Validate_AcceptsZeroMonthlyExpensesAndDepositWhenEntered()
    {
        CreateMortgageApplicationRequest request = new()
        {
            AnnualIncome = 100_000m,
            MonthlyExpenses = 0m,
            DepositAmount = 0m,
            PropertyValue = 400_000m,
            TermYears = 25
        };
        List<ValidationResult> results = new();

        bool isValid = Validator.TryValidateObject(request, new ValidationContext(request), results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void Validate_TermBelowOneYear_AttachesErrorToTermField()
    {
        CreateMortgageApplicationRequest request = new()
        {
            AnnualIncome = 100_000m,
            MonthlyExpenses = 1_200m,
            DepositAmount = 100_000m,
            PropertyValue = 400_000m,
            TermYears = 0
        };
        List<ValidationResult> results = new();

        bool isValid = Validator.TryValidateObject(request, new ValidationContext(request), results, true);

        Assert.False(isValid);
        ValidationResult error = Assert.Single(results);
        Assert.Equal("Enter a term of at least one year.", error.ErrorMessage);
        Assert.Equal([nameof(request.TermYears)], error.MemberNames);
    }
}
