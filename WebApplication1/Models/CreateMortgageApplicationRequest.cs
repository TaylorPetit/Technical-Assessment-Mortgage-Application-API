using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public sealed class CreateMortgageApplicationRequest : IValidatableObject
{
    [Range(typeof(decimal), "0.01", "1000000000000", ErrorMessage = "Annual income must be greater than zero.")]
    public decimal AnnualIncome { get; init; }

    [Range(typeof(decimal), "0", "1000000000000", ErrorMessage = "Monthly expenses cannot be negative.")]
    public decimal MonthlyExpenses { get; init; }

    [Range(typeof(decimal), "0", "1000000000000", ErrorMessage = "Deposit amount cannot be negative.")]
    public decimal DepositAmount { get; init; }

    [Range(typeof(decimal), "0.01", "1000000000000", ErrorMessage = "Property value must be greater than zero.")]
    public decimal PropertyValue { get; init; }

    [Range(1, 40, ErrorMessage = "Term must be between 1 and 40 years.")]
    public int TermYears { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DepositAmount >= PropertyValue)
        {
            yield return new ValidationResult(
                "Deposit amount must be less than property value.",
                [nameof(DepositAmount), nameof(PropertyValue)]);
        }
    }
}
