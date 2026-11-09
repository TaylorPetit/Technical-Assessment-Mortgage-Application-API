using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class CreateMortgageApplicationRequest : IValidatableObject
{
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Annual income must be greater than zero.")]
    public required decimal AnnualIncome { get; init; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Monthly expenses cannot be negative.")]
    public required decimal MonthlyExpenses { get; init; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Deposit amount cannot be negative.")]
    public required decimal DepositAmount { get; init; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Property value must be greater than zero.")]
    public required decimal PropertyValue { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Term must be greater than zero.")]
    public required int TermYears { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DepositAmount > PropertyValue)
        {
            yield return new ValidationResult(
                "Deposit amount cannot exceed property value.",
                [nameof(DepositAmount), nameof(PropertyValue)]);
        }
    }
}
