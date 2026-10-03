using ThaiReference.DataAccess;

namespace ThaiReference.Utilities;

public interface IDataServiceFactory
{
    IDataService Get(string dbName);
    Task<IDataService> GetAsync(string dbName);
}

public class DataServiceFactory(IServiceProvider services) : IDataServiceFactory
{
    private readonly Dictionary<string, IDataService> _cache = [];

    public IDataService Get(string dbName) => GetOrCreate(dbName);

    public async Task<IDataService> GetAsync(string dbName)
    {
        var service = GetOrCreate(dbName);
        if (!service.IsInitialized)
            await service.InitializeAsync();
        return service;
    }

    private IDataService GetOrCreate(string dbName)
    {
        if (_cache.TryGetValue(dbName, out var cached))
            return cached;

        IDataService service = services.GetRequiredService<ApiDataService>();
        service.DbName = dbName;
        _cache[dbName] = service;
        return service;
    }
}
