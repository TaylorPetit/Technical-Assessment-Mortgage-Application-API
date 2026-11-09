using System.Net;
using System.Net.Http.Json;
using MortgageApplication.Web.Models;

namespace MortgageApplication.Web.Services;

public class MortgageApplicationApiClient(HttpClient httpClient)
{
    public async Task<MortgageApplicationResponse?> CreateAsync(
        CreateMortgageApplicationRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "api/mortgage-applications", request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            throw new InvalidOperationException("The API rejected the application details. Check the values and try again.");
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<MortgageApplicationResponse>(cancellationToken);
    }

    public Task<MortgageApplicationResponse?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        httpClient.GetFromJsonAsync<MortgageApplicationResponse>(
            $"api/mortgage-applications/{id}", cancellationToken);

    public Task<AffordabilityAssessmentResponse?> AssessAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        httpClient.GetFromJsonAsync<AffordabilityAssessmentResponse>(
            $"api/mortgage-applications/{id}/affordability", cancellationToken);
}
