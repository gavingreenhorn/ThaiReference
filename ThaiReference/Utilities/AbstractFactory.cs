namespace ThaiReference.Utilities;

public static class AbstractFactoryExtension
{
    public static void AddAbstractFactory<TInterface, TImplementation>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddTransient<TInterface, TImplementation>();
        services.AddSingleton<Func<TInterface>>(x => () => x.GetService<TInterface>()!);
        services.AddSingleton<IAbstractFactory<TInterface>, AbstractFactory<TInterface>>();
    }
}

public class AbstractFactory<T>(Func<T> factory) : IAbstractFactory<T>
{
    private readonly Func<T> _factory = factory;
    public T Create() => _factory();
}

interface IAbstractFactory<T>
{
    public T Create();
}
