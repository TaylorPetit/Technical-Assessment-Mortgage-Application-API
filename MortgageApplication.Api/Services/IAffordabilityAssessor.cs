using MortgageApplicationApi.Models;

namespace MortgageApplicationApi.Services;

public interface IAffordabilityAssessor
{
    AffordabilityAssessment Assess(MortgageApplication application);
}
