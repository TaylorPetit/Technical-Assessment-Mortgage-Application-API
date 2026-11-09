using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IMortgageApplicationRepository
{
    Task AddAsync(MortgageApplication application, CancellationToken cancellationToken = default);
    Task<MortgageApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
