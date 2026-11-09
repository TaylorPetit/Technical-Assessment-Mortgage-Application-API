using System.Text.Json;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

/// <summary>
/// Small file-backed store for this single-instance exercise implementation.
/// </summary>
public class JsonMortgageApplicationRepository : IMortgageApplicationRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public JsonMortgageApplicationRepository(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = Path.GetFullPath(filePath);
    }

    public async Task AddAsync(MortgageApplication application, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var applications = await ReadAllAsync(cancellationToken);
            applications.Add(application);

            var directory = Path.GetDirectoryName(_filePath)!;
            Directory.CreateDirectory(directory);

            var temporaryPath = $"{_filePath}.{Guid.NewGuid():N}.tmp";
            try
            {
                var json = JsonSerializer.Serialize(applications, JsonOptions);
                await File.WriteAllTextAsync(temporaryPath, json, cancellationToken);
                File.Move(temporaryPath, _filePath, overwrite: true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<MortgageApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return (await ReadAllAsync(cancellationToken)).FirstOrDefault(application => application.Id == id);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<MortgageApplication>> ReadAllAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(_filePath);
        return await JsonSerializer.DeserializeAsync<List<MortgageApplication>>(stream, JsonOptions, cancellationToken)
            ?? [];
    }
}
