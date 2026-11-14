using ThaiReference.DataAccess;

static async Task<bool> IsApiHealthyAsync(string baseUrl)
{
    using var http = new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(3) };
    try
    {
        var response = await http.GetAsync("actuator/health");
        return response.IsSuccessStatusCode;
    }
    catch
    {
        return false;
    }
}

string? outPath = null;
string? configPath = null;
string? apiBase = null;
var allowOffline = false;
for (var i = 0; i < args.Length; i++)
{
    if (args[i] == "--out" && i + 1 < args.Length) { outPath = args[++i]; continue; }
    if (args[i] == "--config" && i + 1 < args.Length) { configPath = args[++i]; continue; }
    if (args[i] == "--api" && i + 1 < args.Length) { apiBase = args[++i]; continue; }
    if (args[i] == "--allow-offline") { allowOffline = true; continue; }
}

string baseUrl;
if (!string.IsNullOrEmpty(apiBase))
    baseUrl = apiBase.TrimEnd('/');
else if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath))
{
    var json = await File.ReadAllTextAsync(configPath);
    using var doc = System.Text.Json.JsonDocument.Parse(json);
    var root = doc.RootElement;
    var host = root.TryGetProperty("ApiHost", out var h) ? h.GetString() ?? "127.0.0.1" : "127.0.0.1";
    var port = root.TryGetProperty("ApiPort", out var p) ? p.GetString() ?? "5000" : "5000";
    baseUrl = $"http://{host}:{port}";
}
else
{
    Console.Error.WriteLine("Usage: FetchLiteDb --out <path-to-thai_lite.db> (--api <base-url> | --config <path-to-appsettings.json>)");
    Environment.Exit(1);
    return;
}

if (string.IsNullOrEmpty(outPath))
{
    Console.Error.WriteLine("Usage: FetchLiteDb --out <path-to-thai_lite.db> (--api <base-url> | --config <path-to-appsettings.json>)");
    Environment.Exit(1);
    return;
}

var api = new ApiDataService(baseUrl);
if (!await IsApiHealthyAsync(baseUrl))
{
    if (allowOffline)
    {
        var state = File.Exists(outPath)
            ? $"reusing EXISTING (possibly stale) {outPath}"
            : $"no {outPath} will be packaged; the app will have no offline fallback";
        Console.Error.WriteLine($"warning: API unavailable at {baseUrl}; --allow-offline set, {state}.");
        Environment.Exit(0);
    }

    Console.Error.WriteLine(
        $"API unavailable at {baseUrl} (health check failed); pass --allow-offline to build anyway.");
    Environment.Exit(1);
}

try
{
    await ApiMigrate.Do(outPath, api);
    Console.WriteLine($"Wrote {outPath}");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Fetch failed: {ex.Message}. Existing {outPath} was left unchanged.");
    Environment.Exit(1);
}
