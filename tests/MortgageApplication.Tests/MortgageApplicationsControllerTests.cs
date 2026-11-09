using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MortgageApplicationApi.Controllers;
using MortgageApplicationApi.Models;
using MortgageApplicationApi.Repositories;
using MortgageApplicationApi.Services;
using MortgageApplicationModel = MortgageApplicationApi.Models.MortgageApplication;

namespace MortgageApplication.Tests;

public class MortgageApplicationsControllerTests
{
    [Fact]
    public async Task Assess_Returns422WhenDecimalCalculationOverflows()
    {
        var application = new MortgageApplicationModel(
            Guid.NewGuid(),
            0.01m,
            decimal.MaxValue,
            0m,
            1_000m,
            1,
            DateTimeOffset.UtcNow);
        var repository = new InMemoryMortgageApplicationRepository(application);
        var service = new MortgageApplicationService(repository, new AffordabilityAssessor());
        var controller = new MortgageApplicationsController(service);

        var action = await controller.Assess(application.Id, CancellationToken.None);

        var result = Assert.IsType<ObjectResult>(action.Result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal("Affordability assessment cannot be calculated", problem.Title);
    }

    private class InMemoryMortgageApplicationRepository(MortgageApplicationModel application)
        : IMortgageApplicationRepository
    {
        public Task AddAsync(MortgageApplicationModel application, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<MortgageApplicationModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(id == application.Id ? application : null);
    }
}
