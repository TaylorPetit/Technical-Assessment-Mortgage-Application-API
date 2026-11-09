using System.Text.Json;
using MortgageApplicationApi.Models;
using MortgageApplicationApi.Repositories;
using MortgageApplicationModel = MortgageApplicationApi.Models.MortgageApplication;

namespace MortgageApplication.Tests;

public class JsonMortgageApplicationRepositoryTests
{
    [Fact]
    public async Task GetById_ReturnsNullWhenStoreDoesNotExist()
    {
        var (directory, filePath) = CreateTemporaryStorePath();
        try
        {
            var repository = new JsonMortgageApplicationRepository(filePath);

            var result = await repository.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
            Assert.False(File.Exists(filePath));
        }
        finally
        {
            DeleteTemporaryDirectory(directory);
        }
    }

    [Fact]
    public async Task AddAsync_PersistsApplicationForANewRepositoryInstance()
    {
        var (directory, filePath) = CreateTemporaryStorePath();
        try
        {
            var application = CreateApplication();
            var repository = new JsonMortgageApplicationRepository(filePath);

            await repository.AddAsync(application);

            var json = await File.ReadAllTextAsync(filePath);
            var reloadedApplication = await new JsonMortgageApplicationRepository(filePath)
                .GetByIdAsync(application.Id);

            Assert.Contains("\"annualIncome\"", json);
            Assert.Equal(application, reloadedApplication);
        }
        finally
        {
            DeleteTemporaryDirectory(directory);
        }
    }

    [Fact]
    public async Task AddAsync_PreservesAllApplicationsWhenWritesRunConcurrently()
    {
        var (directory, filePath) = CreateTemporaryStorePath();
        try
        {
            var repository = new JsonMortgageApplicationRepository(filePath);
            var applications = Enumerable.Range(0, 20).Select(_ => CreateApplication()).ToArray();

            await Task.WhenAll(applications.Select(application => repository.AddAsync(application)));

            var reloadedRepository = new JsonMortgageApplicationRepository(filePath);
            foreach (var application in applications)
            {
                Assert.Equal(application, await reloadedRepository.GetByIdAsync(application.Id));
            }
        }
        finally
        {
            DeleteTemporaryDirectory(directory);
        }
    }

    [Fact]
    public async Task GetById_ThrowsWhenStoreContainsInvalidJson()
    {
        var (directory, filePath) = CreateTemporaryStorePath();
        try
        {
            Directory.CreateDirectory(directory);
            await File.WriteAllTextAsync(filePath, "not valid json");
            var repository = new JsonMortgageApplicationRepository(filePath);

            await Assert.ThrowsAsync<JsonException>(() => repository.GetByIdAsync(Guid.NewGuid()));
        }
        finally
        {
            DeleteTemporaryDirectory(directory);
        }
    }

    [Fact]
    public async Task AddAsync_RespectsCancellationBeforeWriting()
    {
        var (directory, filePath) = CreateTemporaryStorePath();
        try
        {
            var repository = new JsonMortgageApplicationRepository(filePath);
            using var cancellation = new CancellationTokenSource();
            await cancellation.CancelAsync();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => repository.AddAsync(CreateApplication(), cancellation.Token));

            Assert.False(File.Exists(filePath));
        }
        finally
        {
            DeleteTemporaryDirectory(directory);
        }
    }

    private static MortgageApplicationModel CreateApplication() => new(
        Guid.NewGuid(),
        100_000m,
        1_200m,
        100_000m,
        400_000m,
        25,
        DateTimeOffset.UtcNow);

    private static (string Directory, string FilePath) CreateTemporaryStorePath()
    {
        var root = Path.Combine(AppContext.BaseDirectory, "RepositoryTestData");
        var directory = Path.Combine(root, $"MortgageApplicationTests-{Guid.NewGuid():N}");
        return (directory, Path.Combine(directory, "applications.json"));
    }

    private static void DeleteTemporaryDirectory(string directory)
    {
        var testDataRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "RepositoryTestData"))
            + Path.DirectorySeparatorChar;
        var fullPath = Path.GetFullPath(directory);
        if (!fullPath.StartsWith(testDataRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Refusing to remove a directory outside the repository test data folder.");
        }

        if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, recursive: true);
        }
    }
}
