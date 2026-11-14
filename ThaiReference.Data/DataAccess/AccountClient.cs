using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace ThaiReference.DataAccess;

public sealed class AccountClient
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    readonly HttpClient _http;

    public AccountClient(IConfiguration config)
        : this(ApiDataService.GetBaseUrl(config))
    {
    }

    public AccountClient(string baseUrl)
        : this(new HttpClient { BaseAddress = new Uri(baseUrl), Timeout = TimeSpan.FromSeconds(8) })
    {
    }

    public AccountClient(HttpClient http)
    {
        _http = http;
    }

    public Task<AccountInfoResponse?> CheckinAsync(
        string uuid, string? platform, string? build, string? model, CancellationToken cancellationToken = default) =>
        SendAsync<AccountInfoResponse>(HttpMethod.Post, "/account/checkin", uuid, platform, build, null, cancellationToken, model);

    public Task<AccountInfoResponse?> GetInfoAsync(
        string uuid, string? platform, string? build, CancellationToken cancellationToken = default) =>
        SendAsync<AccountInfoResponse>(HttpMethod.Get, "/account/info", uuid, platform, build, null, cancellationToken);

    public Task<PairStartResponse?> StartPairAsync(
        string uuid, string? platform, string? build, CancellationToken cancellationToken = default) =>
        SendAsync<PairStartResponse>(HttpMethod.Post, "/account/devices/pair/start", uuid, platform, build, null, cancellationToken);

    public Task<AccountInfoResponse?> CompletePairAsync(
        string uuid, string token, string? platform, string? build, CancellationToken cancellationToken = default) =>
        SendAsync<AccountInfoResponse>(
            HttpMethod.Post,
            "/account/devices/pair/complete",
            uuid,
            platform,
            build,
            new PairCompleteRequest(token),
            cancellationToken);

    public Task<AccountInfoResponse?> UnlinkAsync(
        string uuid, Guid deviceUuid, string? platform, string? build, CancellationToken cancellationToken = default) =>
        SendAsync<AccountInfoResponse>(
            HttpMethod.Post,
            "/account/devices/unlink",
            uuid,
            platform,
            build,
            new UnlinkRequest(deviceUuid),
            cancellationToken);

    public Task<AccountInfoResponse?> TransferOwnerAsync(
        string uuid, Guid deviceUuid, string? platform, string? build, CancellationToken cancellationToken = default) =>
        SendAsync<AccountInfoResponse>(
            HttpMethod.Post, "/account/devices/owner", uuid, platform, build,
            new UnlinkRequest(deviceUuid), cancellationToken);

    public Task<OkResponse?> RequestEmailAsync(
        string uuid, string email, string? platform, string? build, CancellationToken cancellationToken = default) =>
        SendAsync<OkResponse>(
            HttpMethod.Post,
            "/account/email",
            uuid,
            platform,
            build,
            new EmailRequest(email),
            cancellationToken);

    public Task<OkResponse?> ConfirmEmailAsync(
        string uuid, string code, string? platform, string? build, CancellationToken cancellationToken = default) =>
        SendAsync<OkResponse>(
            HttpMethod.Post,
            "/account/email/confirm",
            uuid,
            platform,
            build,
            new CodeRequest(code),
            cancellationToken);

    public Task<OkResponse?> RequestRecoverAsync(
        string uuid, string email, string? platform, string? build, CancellationToken cancellationToken = default) =>
        SendAsync<OkResponse>(
            HttpMethod.Post,
            "/account/recover",
            uuid,
            platform,
            build,
            new EmailRequest(email),
            cancellationToken);

    public Task<OkResponse?> ConfirmRecoverAsync(
        string uuid, string code, string? platform, string? build, CancellationToken cancellationToken = default) =>
        SendAsync<OkResponse>(
            HttpMethod.Post,
            "/account/recover/confirm",
            uuid,
            platform,
            build,
            new CodeRequest(code),
            cancellationToken);

    async Task<T?> SendAsync<T>(
        HttpMethod method,
        string path,
        string uuid,
        string? platform,
        string? build,
        object? body,
        CancellationToken cancellationToken,
        string? model = null)
        where T : class
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.TryAddWithoutValidation("Authorization", uuid);
        if (!string.IsNullOrWhiteSpace(platform))
            request.Headers.TryAddWithoutValidation("X-Platform", platform);
        if (!string.IsNullOrWhiteSpace(build))
            request.Headers.TryAddWithoutValidation("X-Build-Number", build);
        if (!string.IsNullOrWhiteSpace(model))
            request.Headers.TryAddWithoutValidation("X-Device-Model", model);
        if (body is not null)
            request.Content = JsonContent.Create(body, options: JsonOptions);

        using var response = await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            return null;
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken).ConfigureAwait(false);
    }

    public record AccountInfoResponse(
        bool Created,
        long GroupId,
        Guid DeviceUuid,
        bool EmailSet,
        string? Email,
        bool Owner,
        List<DeviceMemberDto> Devices);

    public record DeviceMemberDto(
        Guid Uuid,
        string? Model,
        string? Platform,
        string? Build,
        DateTimeOffset? LinkedAt,
        DateTimeOffset? LastSeen,
        bool ThisDevice,
        bool Owner,
        string? LastIp);

    public record PairStartResponse(string Token, DateTimeOffset ExpiresAt);

    public record OkResponse(bool Ok);

    record PairCompleteRequest(string Token);

    record UnlinkRequest(Guid DeviceUuid);

    record EmailRequest(string Email);

    record CodeRequest(string Code);
}
