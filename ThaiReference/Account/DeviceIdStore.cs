using System.Security.Cryptography;
using System.Text;
using Android.Provider;
using Application = Android.App.Application;

namespace ThaiReference.Account;

public sealed class DeviceIdStore
{
    public Task<string> GetOrCreateAsync() => Task.FromResult(FromAndroidId());

    static string FromAndroidId()
    {
        var androidId = Settings.Secure.GetString(
            Application.Context.ContentResolver,
            Settings.Secure.AndroidId);
        if (string.IsNullOrWhiteSpace(androidId))
            throw new InvalidOperationException("ANDROID_ID is unavailable.");
        return ToUuidV4(androidId);
    }

    static string ToUuidV4(string input)
    {
        var hex = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input))).ToLowerInvariant();
        var p1 = hex[..8];
        var p2 = hex[8..12];
        var p3 = hex[12..16];
        var p4 = hex[16..20];
        var p5 = hex[20..32];
        var p3Fixed = "4" + p3[1..];
        var nibble = Convert.ToInt32(p4[..1], 16);
        var p4Fixed = ((nibble & 0x3) | 0x8).ToString("x") + p4[1..];
        return $"{p1}-{p2}-{p3Fixed}-{p4Fixed}-{p5}";
    }
}
