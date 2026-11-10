using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using MortgageApplicationApi.Controllers;
using MortgageApplicationApi.Models;
using MortgageApplicationApi.Repositories;
using MortgageApplicationApi.Services;
using MortgageApplicationModel = MortgageApplicationApi.Models.MortgageApplication;

namespace MortgageApplication.Tests;

public class MortgageApplicationsControllerTests
{
    [Fact]
    public async Task Create_ReturnsCreatedLocationAndSavesApplication()
    {
        InMemoryMortgageApplicationRepository repository = new();
        MortgageApplicationService service = new(repository, new AffordabilityAssessor());
        MortgageApplicationsController controller = new(service, NullLogger<MortgageApplicationsController>.Instance);
        CreateMortgageApplicationRequest request = new()
        {
            AnnualIncome = 100_000m,
            MonthlyExpenses = 1_200m,
            DepositAmount = 100_000m,
            PropertyValue = 400_000m,
            TermYears = 25
        };

        ActionResult<MortgageApplicationModel> action = await controller.Create(request, CancellationToken.None);

        CreatedAtActionResult result = Assert.IsType<CreatedAtActionResult>(action.Result);
        MortgageApplicationModel application = Assert.IsType<MortgageApplicationModel>(result.Value);
        Assert.Equal(nameof(MortgageApplicationsController.GetById), result.ActionName);
        Assert.Equal(application.Id, result.RouteValues!["id"]);
        Assert.Same(application, await repository.GetByIdAsync(application.Id));
    }

    [Fact]
    public async Task GetById_ReturnsApplicationWhenFound()
    {
        MortgageApplicationModel application = CreateApplication();
        MortgageApplicationsController controller = CreateController(new InMemoryMortgageApplicationRepository(application));

        ActionResult<MortgageApplicationModel> action = await controller.GetById(application.Id, CancellationToken.None);

        OkObjectResult result = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Same(application, result.Value);
    }

    [Fact]
    public async Task GetById_ReturnsNotFoundWhenMissing()
    {
        MortgageApplicationsController controller = CreateController(new InMemoryMortgageApplicationRepository());

        ActionResult<MortgageApplicationModel> action = await controller.GetById(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(action.Result);
    }

    [Fact]
    public async Task Assess_ReturnsAssessmentWhenApplicationExists()
    {
        MortgageApplicationModel application = CreateApplication();
        MortgageApplicationsController controller = CreateController(new InMemoryMortgageApplicationRepository(application));

        ActionResult<AffordabilityAssessment> action = await controller.Assess(application.Id, CancellationToken.None);

        OkObjectResult result = Assert.IsType<OkObjectResult>(action.Result);
        AffordabilityAssessment assessment = Assert.IsType<AffordabilityAssessment>(result.Value);
        Assert.Equal(application.Id, assessment.ApplicationId);
    }

    [Fact]
    public async Task Assess_ReturnsNotFoundWhenApplicationIsMissing()
    {
        MortgageApplicationsController controller = CreateController(new InMemoryMortgageApplicationRepository());

        ActionResult<AffordabilityAssessment> action = await controller.Assess(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(action.Result);
    }

    [Fact]
    public async Task Assess_Returns422WhenDecimalCalculationOverflows()
    {
        MortgageApplicationModel application = CreateApplication(annualIncome: 0.01m, monthlyExpenses: decimal.MaxValue);
        MortgageApplicationsController controller = CreateController(new InMemoryMortgageApplicationRepository(application));

        ActionResult<AffordabilityAssessment> action = await controller.Assess(application.Id, CancellationToken.None);

        ObjectResult result = Assert.IsType<ObjectResult>(action.Result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.StatusCode);
        ProblemDetails problem = Assert.IsType<ProblemDetails>(result.Value);
        Assert.Equal("Affordability assessment cannot be calculated", problem.Title);
    }

    private static MortgageApplicationModel CreateApplication(
        decimal annualIncome = 100_000m,
        decimal monthlyExpenses = 1_200m) => new(
            Guid.NewGuid(),
            annualIncome,
            monthlyExpenses,
            100_000m,
            400_000m,
            25,
            DateTimeOffset.UtcNow);

    private static MortgageApplicationsController CreateController(IMortgageApplicationRepository repository) =>
        new(new MortgageApplicationService(repository, new AffordabilityAssessor()),
            NullLogger<MortgageApplicationsController>.Instance);

    private class InMemoryMortgageApplicationRepository : IMortgageApplicationRepository
    {
        private readonly List<MortgageApplicationModel> _applications;

        public InMemoryMortgageApplicationRepository(params MortgageApplicationModel[] applications)
        {
            _applications = [.. applications];
        }

        public Task AddAsync(MortgageApplicationModel application, CancellationToken cancellationToken = default)
        {
            _applications.Add(application);
            return Task.CompletedTask;
        }

        public Task<MortgageApplicationModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_applications.FirstOrDefault(application => application.Id == id));
    }
}
