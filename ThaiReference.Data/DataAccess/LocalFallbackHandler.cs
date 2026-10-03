namespace ThaiReference.DataAccess;

public sealed class LocalFallbackHandler
{
	public static IReadOnlyList<T> GetList<T>(
		Func<List<T>> fetchFromApi,
		Func<IEnumerable<T>> fromFallback,
		bool preferFallbackWhenApiEmpty = false)
	{
		try
		{
			var fromApi = fetchFromApi() ?? [];
			if (preferFallbackWhenApiEmpty && fromApi.Count == 0)
			{
				var local = fromFallback().ToList();
				if (local.Count > 0)
					return local;
			}
			return fromApi;
		}
		catch (Exception)
		{
			return fromFallback().ToList();
		}
	}

	public static T GetValue<T>(Func<T> fetchFromApi, Func<T> fromFallback)
	{
		try
		{
			return fetchFromApi();
		}
		catch (Exception)
		{
			return fromFallback();
		}
	}
}
