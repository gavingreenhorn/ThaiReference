using Microsoft.Extensions.Configuration;

namespace ThaiReference.DataAccess;

public static class DataServiceRegistration
{
	public static IServiceCollection AddThaiDataServices(this IServiceCollection services)
	{
		static HttpClient ApiHttpClient(IConfiguration config) =>
			new()
			{
				BaseAddress = new Uri(ApiDataService.GetBaseUrl(config)),
				Timeout = TimeSpan.FromSeconds(1.5),
			};

		services.AddSingleton(sp =>
		{
			var config = sp.GetRequiredService<IConfiguration>();
			var healthEndpoint = config["HealthEndpoint"]
				?? throw new InvalidOperationException("HealthEndpoint missing from configuration");
			return new ApiAvailabilityMonitor(ApiHttpClient(config), healthEndpoint);
		});

		services.AddTransient(sp =>
			new ApiDataService(
				ApiHttpClient(sp.GetRequiredService<IConfiguration>()),
				FileSystem.AppDataDirectory,
				EnsureSnapshotDbAsync,
				sp.GetRequiredService<ApiAvailabilityMonitor>()
			)
		);

		return services;
	}

	/// <summary>
	/// Copy packaged LiteDB into AppData when missing, tiny, or missing required collections.
	/// Runs fully async with ConfigureAwait(false) so sync-over-async from the UI thread cannot deadlock.
	/// </summary>
	static async Task EnsureSnapshotDbAsync(string dbFileName)
	{
		var dest = Path.Combine(FileSystem.AppDataDirectory, dbFileName);

		bool needsCopy =
			!File.Exists(dest)
			|| new FileInfo(dest).Length < OfflineSnapshot.MinBytes
			|| !OfflineSnapshot.HasRequiredCollections(dest);

		if (!needsCopy)
			return;

		if (!await FileSystem.AppPackageFileExistsAsync(dbFileName).ConfigureAwait(false))
		{
			System.Diagnostics.Debug.WriteLine(
				$"[EnsureSnapshotDb] Packaged '{dbFileName}' not found in app package.");
			return;
		}

		await using var stream = await FileSystem.OpenAppPackageFileAsync(dbFileName).ConfigureAwait(false);
		using var memoryStream = new MemoryStream();
		await stream.CopyToAsync(memoryStream).ConfigureAwait(false);

		if (memoryStream.Length < OfflineSnapshot.MinBytes)
		{
			System.Diagnostics.Debug.WriteLine(
				$"[EnsureSnapshotDb] Packaged '{dbFileName}' is too small ({memoryStream.Length} bytes).");
			return;
		}

		var directory = Path.GetDirectoryName(dest);
		if (!string.IsNullOrEmpty(directory))
			Directory.CreateDirectory(directory);

		if (File.Exists(dest))
		{
			try { File.Delete(dest); }
			catch (IOException ex)
			{
				System.Diagnostics.Debug.WriteLine(
					$"[EnsureSnapshotDb] Could not delete stale '{dest}': {ex.Message}");
				return;
			}
		}

		var temp = dest + ".tmp";
		await File.WriteAllBytesAsync(temp, memoryStream.ToArray()).ConfigureAwait(false);
		File.Move(temp, dest, overwrite: true);

		System.Diagnostics.Debug.WriteLine(
			$"[EnsureSnapshotDb] Copied '{dbFileName}' ({memoryStream.Length} bytes) to AppData.");
	}
}
