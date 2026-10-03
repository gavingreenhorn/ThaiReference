using System.Net;
using System.Net.Http.Headers;
using LiteDB;
using ThaiReference.DataAccess;
using Xunit;

namespace ThaiReference.Tests;

/// <summary>Packaged snapshot DB path for the suite.</summary>
public sealed class SnapshotDbFixture
{
	public string PackagedDbPath { get; }

	public SnapshotDbFixture()
	{
		PackagedDbPath = Path.Combine(AppContext.BaseDirectory, "thai_lite.db");
		Assert.True(File.Exists(PackagedDbPath), $"Missing packaged snapshot: {PackagedDbPath}");
	}
}

[CollectionDefinition(nameof(SnapshotDbCollection))]
public class SnapshotDbCollection : ICollectionFixture<SnapshotDbFixture>;

[Collection(nameof(SnapshotDbCollection))]
public class BackendDownFallbackTests
{
	const string HealthEndpoint = "actuator/health";

	readonly SnapshotDbFixture _snapshot;

	public BackendDownFallbackTests(SnapshotDbFixture snapshot) =>
		_snapshot = snapshot;

	[Fact]
	public void When_api_unreachable_ensure_copies_snapshot_then_serves_records()
	{
		var snapshotDbDir = Path.Combine(Path.GetTempPath(), "tr_snap_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(snapshotDbDir);
		var destDbPath = Path.Combine(snapshotDbDir, "thai_lite.db");

		int expectedConsonants;
		int expectedVowels;
		using (var db = new LiteDatabase(_snapshot.PackagedDbPath))
		{
			expectedConsonants = db.GetCollection("consonants").Count();
			expectedVowels = db.GetCollection("vowels").Count();
		}
		Assert.True(expectedConsonants > 0);
		Assert.True(expectedVowels > 0);

		Task EnsureAsync(string dbFileName)
		{
			var dest = Path.Combine(snapshotDbDir, dbFileName);
			if (!File.Exists(dest))
				File.Copy(_snapshot.PackagedDbPath, dest);
			return Task.CompletedTask;
		}

		using var http = new HttpClient
		{
			BaseAddress = new Uri("http://127.0.0.1:1"),
			Timeout = TimeSpan.FromMilliseconds(500),
		};
		var api = new ApiDataService(http, snapshotDbDir, EnsureAsync) { DbName = "thai" };

		Assert.False(File.Exists(destDbPath));
		Assert.Equal(expectedConsonants, api.Consonants.Count());
		Assert.True(File.Exists(destDbPath));
		Assert.Equal(expectedVowels, api.Vowels.Count());
	}

	[Fact]
	public async Task CheckOnce_sets_IsUp_false_when_health_fails()
	{
		using var http = new HttpClient(new SequenceHandler([(HttpStatusCode)0]))
		{
			BaseAddress = new Uri("http://127.0.0.1:1"),
			Timeout = TimeSpan.FromMilliseconds(200),
		};
		var monitor = new ApiAvailabilityMonitor(http, HealthEndpoint);
		Assert.False(monitor.IsUp); // cold start defaults offline

		var changes = new List<bool>();
		monitor.Changed += changes.Add;

		await monitor.CheckOnceAsync();

		Assert.False(monitor.IsUp);
		Assert.Empty(changes); // already down — no edge

		await monitor.CheckOnceAsync();
		Assert.Empty(changes);
	}

	[Fact]
	public void When_appdata_has_empty_stub_db_ensure_replaces_then_serves_directions()
	{
		var snapshotDbDir = Path.Combine(Path.GetTempPath(), "tr_snap_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(snapshotDbDir);
		var destDbPath = Path.Combine(snapshotDbDir, "thai_lite.db");

		// Simulate the old bug: LiteDB created an empty AppData file that blocked re-copy.
		using (var empty = new LiteDatabase(destDbPath))
			_ = empty.GetCollection("directions");

		Assert.True(File.Exists(destDbPath));
		Assert.False(OfflineSnapshot.HasRequiredCollections(destDbPath));

		int expectedDirections;
		using (var db = new LiteDatabase(_snapshot.PackagedDbPath))
			expectedDirections = db.GetCollection("directions").Count();
		Assert.True(expectedDirections > 0);

		Task EnsureAsync(string dbFileName)
		{
			var dest = Path.Combine(snapshotDbDir, dbFileName);
			bool needs =
				!File.Exists(dest)
				|| new FileInfo(dest).Length < OfflineSnapshot.MinBytes
				|| !OfflineSnapshot.HasRequiredCollections(dest);
			if (!needs)
				return Task.CompletedTask;
			if (File.Exists(dest))
				File.Delete(dest);
			File.Copy(_snapshot.PackagedDbPath, dest);
			return Task.CompletedTask;
		}

		using var http = new HttpClient
		{
			BaseAddress = new Uri("http://127.0.0.1:1"),
			Timeout = TimeSpan.FromMilliseconds(500),
		};
		var api = new ApiDataService(http, snapshotDbDir, EnsureAsync) { DbName = "thai" };

		Assert.Equal(expectedDirections, api.Directions.Count());
		Assert.True(OfflineSnapshot.HasRequiredCollections(destDbPath));
	}

	[Fact]
	public async Task CheckOnce_sets_IsUp_true_when_health_ok()
	{
		using var http = new HttpClient(new SequenceHandler([HttpStatusCode.OK]))
		{
			BaseAddress = new Uri("http://example.test/"),
		};
		var monitor = new ApiAvailabilityMonitor(http, HealthEndpoint);
		monitor.MarkDown();

		await monitor.CheckOnceAsync();

		Assert.True(monitor.IsUp);
	}

	[Fact]
	public void When_down_skips_http_and_serves_snapshot()
	{
		var snapshotDbDir = Path.Combine(Path.GetTempPath(), "tr_snap_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(snapshotDbDir);

		int expectedConsonants;
		using (var db = new LiteDatabase(_snapshot.PackagedDbPath))
			expectedConsonants = db.GetCollection("consonants").Count();

		Task EnsureAsync(string dbFileName)
		{
			var dest = Path.Combine(snapshotDbDir, dbFileName);
			if (!File.Exists(dest))
				File.Copy(_snapshot.PackagedDbPath, dest);
			return Task.CompletedTask;
		}

		var counting = new CountingHandler(_ =>
			new HttpResponseMessage(HttpStatusCode.OK));
		using var http = new HttpClient(counting) { BaseAddress = new Uri("http://example.test/") };
		var monitor = new ApiAvailabilityMonitor(
			new HttpClient { BaseAddress = new Uri("http://example.test/") },
			HealthEndpoint);
		monitor.MarkDown();

		var api = new ApiDataService(http, snapshotDbDir, EnsureAsync, monitor) { DbName = "thai" };
		var sw = System.Diagnostics.Stopwatch.StartNew();
		Assert.Equal(expectedConsonants, api.Consonants.Count());
		sw.Stop();

		Assert.Equal(0, counting.SendCount);
		Assert.True(sw.Elapsed < TimeSpan.FromSeconds(1), $"Took {sw.Elapsed}; expected no HTTP wait");
	}

	[Fact]
	public async Task When_back_up_clears_caches_so_next_access_hits_http()
	{
		var snapshotDbDir = Path.Combine(Path.GetTempPath(), "tr_snap_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(snapshotDbDir);
		File.Copy(_snapshot.PackagedDbPath, Path.Combine(snapshotDbDir, "thai_lite.db"));

		var consonantJson = """[{"grapheme":"ก","phoneme":"k","consonant_class":"M","ending":"K","transcription":{"paiboon":"g","ipa":"k"},"initial_sound":{"value":"k","sound_type":"stop","is_consonant_sound":true,"is_initial_only":false,"media_url":null},"ending_sound":null,"sample_words":[]}]""";

		var counting = new CountingHandler(req =>
		{
			if (req.RequestUri!.AbsolutePath.Contains("actuator/health"))
				return new HttpResponseMessage(HttpStatusCode.OK);
			if (req.RequestUri.AbsolutePath.Contains("consonants"))
			{
				var msg = new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(consonantJson),
				};
				msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
				return msg;
			}
			return new HttpResponseMessage(HttpStatusCode.NotFound);
		});

		using var http = new HttpClient(counting) { BaseAddress = new Uri("http://example.test/") };
		var monitor = new ApiAvailabilityMonitor(http, HealthEndpoint);
		monitor.MarkDown();

		var api = new ApiDataService(http, snapshotDbDir, availability: monitor) { DbName = "thai" };
		_ = api.Consonants.Count();
		var afterOffline = counting.SendCount;

		await monitor.CheckOnceAsync();
		Assert.True(monitor.IsUp);

		_ = api.Consonants.Count();
		Assert.True(counting.SendCount > afterOffline, "Expected HTTP after cache clear on up");
	}
}

sealed class SequenceHandler(HttpStatusCode[] codes) : HttpMessageHandler
{
	int _i;

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (_i >= codes.Length || codes[_i] == 0)
			throw new HttpRequestException("unreachable");
		var code = codes[_i++];
		return Task.FromResult(new HttpResponseMessage(code));
	}
}

sealed class CountingHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
{
	public int SendCount { get; private set; }

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		SendCount++;
		return Task.FromResult(respond(request));
	}
}
