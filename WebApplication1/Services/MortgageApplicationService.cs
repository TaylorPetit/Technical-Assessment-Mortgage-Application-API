using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Services;

public class MortgageApplicationService(
    IMortgageApplicationRepository repository,
    AffordabilityAssessor affordabilityAssessor)
{
    public async Task<MortgageApplication> CreateAsync(
        CreateMortgageApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        var application = new MortgageApplication(
            Guid.NewGuid(),
            request.AnnualIncome,
            request.MonthlyExpenses,
            request.DepositAmount,
            request.PropertyValue,
            request.TermYears,
            DateTimeOffset.UtcNow);

        await repository.AddAsync(application, cancellationToken);
        return application;
    }

    public Task<MortgageApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.GetByIdAsync(id, cancellationToken);

    public async Task<AffordabilityAssessment?> AssessAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var application = await repository.GetByIdAsync(id, cancellationToken);
        return application is null ? null : affordabilityAssessor.Assess(application);
    }
}
