using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MortgageApplicationApi.Models;
using MortgageApplicationApi.Services;

namespace MortgageApplicationApi.Controllers;

[ApiController]
[Authorize]
[Route("api/mortgage-applications")]
public class MortgageApplicationsController(
    MortgageApplicationService service,
    ILogger<MortgageApplicationsController> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<MortgageApplication>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MortgageApplication>> Create(
        [FromBody] CreateMortgageApplicationRequest request,
        CancellationToken cancellationToken)
    {
        MortgageApplication application = await service.CreateAsync(request, cancellationToken);
        logger.LogInformation("Mortgage application created with ID {ApplicationId}.", application.Id);
        return CreatedAtAction(nameof(GetById), new { id = application.Id }, application);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<MortgageApplication>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MortgageApplication>> GetById(Guid id, CancellationToken cancellationToken)
    {
        MortgageApplication? application = await service.GetByIdAsync(id, cancellationToken);
        return application is null ? NotFound() : Ok(application);
    }

    [HttpGet("{id:guid}/affordability")]
    [ProducesResponseType<AffordabilityAssessment>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<AffordabilityAssessment>> Assess(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            AffordabilityAssessment? assessment = await service.AssessAsync(id, cancellationToken);
            return assessment is null ? NotFound() : Ok(assessment);
        }
        catch (OverflowException)
        {
            logger.LogWarning(
                "Affordability assessment for application {ApplicationId} exceeded the supported calculation range.",
                id);
            return Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Affordability assessment cannot be calculated",
                detail: "The financial values exceed the range supported by decimal calculations.");
        }
    }
}
