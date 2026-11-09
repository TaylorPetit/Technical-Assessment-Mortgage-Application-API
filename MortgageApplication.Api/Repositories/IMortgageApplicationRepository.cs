using MortgageApplicationApi.Models;

namespace MortgageApplicationApi.Repositories;

public interface IMortgageApplicationRepository
{
    Task AddAsync(MortgageApplication application, CancellationToken cancellationToken = default);
    Task<MortgageApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
