using System.ComponentModel.DataAnnotations;
using MortgageApplication.Web.Models;

namespace MortgageApplication.Tests;

public class CreateMortgageApplicationFormTests
{
    [Fact]
    public void Validate_DepositAbovePropertyValue_AttachesErrorToDepositField()
    {
        var request = new CreateMortgageApplicationRequest
        {
            AnnualIncome = 100_000m,
            MonthlyExpenses = 1_200m,
            DepositAmount = 500_000m,
            PropertyValue = 400_000m,
            TermYears = 25
        };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, new ValidationContext(request), results, true);

        Assert.False(isValid);
        var error = Assert.Single(results);
        Assert.Equal("Deposit cannot exceed the property value.", error.ErrorMessage);
        Assert.Equal([nameof(request.DepositAmount)], error.MemberNames);
    }

    [Fact]
    public void Validate_DepositWithinPropertyValue_ReturnsValid()
    {
        var request = new CreateMortgageApplicationRequest
        {
            AnnualIncome = 100_000m,
            MonthlyExpenses = 1_200m,
            DepositAmount = 100_000m,
            PropertyValue = 400_000m,
            TermYears = 25
        };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, new ValidationContext(request), results, true);

        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void Validate_TermBelowOneYear_AttachesErrorToTermField()
    {
        var request = new CreateMortgageApplicationRequest
        {
            AnnualIncome = 100_000m,
            MonthlyExpenses = 1_200m,
            DepositAmount = 100_000m,
            PropertyValue = 400_000m,
            TermYears = 0
        };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(request, new ValidationContext(request), results, true);

        Assert.False(isValid);
        var error = Assert.Single(results);
        Assert.Equal("Enter a term of at least one year.", error.ErrorMessage);
        Assert.Equal([nameof(request.TermYears)], error.MemberNames);
    }
}
