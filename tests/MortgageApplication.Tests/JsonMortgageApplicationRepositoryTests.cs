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
        (string directory, string filePath) = CreateTemporaryStorePath();
        try
        {
            JsonMortgageApplicationRepository repository = new(filePath);

            MortgageApplicationModel? result = await repository.GetByIdAsync(Guid.NewGuid());

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
        (string directory, string filePath) = CreateTemporaryStorePath();
        try
        {
            MortgageApplicationModel application = CreateApplication();
            JsonMortgageApplicationRepository repository = new(filePath);

            await repository.AddAsync(application);

            string json = await File.ReadAllTextAsync(filePath);
            MortgageApplicationModel? reloadedApplication = await new JsonMortgageApplicationRepository(filePath)
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
        (string directory, string filePath) = CreateTemporaryStorePath();
        try
        {
            JsonMortgageApplicationRepository repository = new(filePath);
            MortgageApplicationModel[] applications = Enumerable.Range(0, 20).Select(_ => CreateApplication()).ToArray();

            await Task.WhenAll(applications.Select(application => repository.AddAsync(application)));

            JsonMortgageApplicationRepository reloadedRepository = new(filePath);
            foreach (MortgageApplicationModel application in applications)
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
        (string directory, string filePath) = CreateTemporaryStorePath();
        try
        {
            Directory.CreateDirectory(directory);
            await File.WriteAllTextAsync(filePath, "not valid json");
            JsonMortgageApplicationRepository repository = new(filePath);

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
        (string directory, string filePath) = CreateTemporaryStorePath();
        try
        {
            JsonMortgageApplicationRepository repository = new(filePath);
            using CancellationTokenSource cancellation = new();
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
        string root = Path.Combine(AppContext.BaseDirectory, "RepositoryTestData");
        string directory = Path.Combine(root, $"MortgageApplicationTests-{Guid.NewGuid():N}");
        return (directory, Path.Combine(directory, "applications.json"));
    }

    private static void DeleteTemporaryDirectory(string directory)
    {
        string testDataRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "RepositoryTestData"))
            + Path.DirectorySeparatorChar;
        string fullPath = Path.GetFullPath(directory);
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
