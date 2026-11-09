using System.ComponentModel.DataAnnotations;

namespace MortgageApplication.Web.Models;

public class CreateMortgageApplicationRequest
{
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Enter an annual income greater than zero.")]
    public decimal AnnualIncome { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "Monthly expenses cannot be negative.")]
    public decimal MonthlyExpenses { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335", ErrorMessage = "The deposit cannot be negative.")]
    public decimal DepositAmount { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Enter a property value greater than zero.")]
    public decimal PropertyValue { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Enter a term of at least one year.")]
    public int TermYears { get; set; }
}
