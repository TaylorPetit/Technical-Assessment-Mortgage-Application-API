using System.ComponentModel.DataAnnotations;

namespace MortgageApplication.Web.Models;

public class CreateMortgageApplicationRequest : IValidatableObject
{
    [Required(ErrorMessage = "Enter an annual income.")]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Enter an annual income greater than zero.")]
    public decimal? AnnualIncome { get; set; }

    [Required(ErrorMessage = "Enter your monthly expenses. Use zero if you have none.")]
    [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Monthly expenses cannot be negative.")]
    public decimal? MonthlyExpenses { get; set; }

    [Required(ErrorMessage = "Enter your deposit. Use zero if you have none.")]
    [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "The deposit cannot be negative.")]
    public decimal? DepositAmount { get; set; }

    [Required(ErrorMessage = "Enter a property value.")]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Enter a property value greater than zero.")]
    public decimal? PropertyValue { get; set; }

    [Required(ErrorMessage = "Enter a mortgage term.")]
    [Range(1, int.MaxValue, ErrorMessage = "Enter a term of at least one year.")]
    public int? TermYears { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DepositAmount is decimal depositAmount &&
            PropertyValue is decimal propertyValue &&
            depositAmount > propertyValue)
        {
            yield return new ValidationResult(
                "Deposit cannot exceed the property value.",
                [nameof(DepositAmount)]);
        }
    }
}
