namespace ThaiReference.DataAccess;

/// <summary>Shared up/down flag for the API. Polls health every interval; request failures call <see cref="MarkDown"/>.</summary>
public sealed class ApiAvailabilityMonitor(HttpClient http, string healthPath)
{
	/// <summary>True until a health check or request actually fails — the API is the source of truth,
	/// the offline snapshot is only used after a proven failure.</summary>
	public bool IsUp { get; private set; } = true;

	/// <summary>Raised when <see cref="IsUp"/> changes. Argument is the new value.</summary>
	public event Action<bool>? Changed;

	public void MarkDown() => Set(false);

	public async Task CheckOnceAsync(CancellationToken cancellationToken = default)
	{
		try
		{
			using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			timeout.CancelAfter(TimeSpan.FromSeconds(1.5));
			var response = await http.GetAsync(healthPath, timeout.Token).ConfigureAwait(false);
			Set(response.IsSuccessStatusCode);
		}
		catch
		{
			Set(false);
		}
	}

	public void Start(TimeSpan interval) =>
		_ = RunAsync(interval);

	async Task RunAsync(TimeSpan interval)
	{
		using var timer = new PeriodicTimer(interval);
		while (await timer.WaitForNextTickAsync().ConfigureAwait(false))
			await CheckOnceAsync().ConfigureAwait(false);
	}

	void Set(bool up)
	{
		if (IsUp == up)
			return;
		IsUp = up;
		Changed?.Invoke(up);
	}
}
