using ThaiReference.Contracts;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Utilities.Extensions;

public static class VoicedExtensions
{
    public static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };

    public static async Task PlayAudioAsync(this IVoiced voiced, IHaveRecording sound)
    {
        if (
            voiced.AudioManager == null ||
            sound == null ||
            sound.Media == null ||
            sound.Media.Length == 0
        ) return;
        using var stream = new MemoryStream(sound.Media);
        var audioPlayer = voiced.AudioManager.CreatePlayer(stream);
        audioPlayer.Volume = 1.0;
        await Task.Run(() => audioPlayer.Play());
    }

    public static async Task PlayFromUrlAsync(this IVoiced voiced, string url)
    {
        if (voiced.AudioManager == null || string.IsNullOrWhiteSpace(url)) return;
        try
        {
            using var response = await Http.GetAsync(url);
            response.EnsureSuccessStatusCode();
            byte[] bytes = await response.Content.ReadAsByteArrayAsync();
            if (bytes.Length == 0) return;
            using var stream = new MemoryStream(bytes);
            var audioPlayer = voiced.AudioManager.CreatePlayer(stream);
            audioPlayer.Volume = 1.0;
            await Task.Run(() => audioPlayer.Play());
        }
        catch
        {
            // no-op: playback from URL failed
        }
    }

    private static Locale? _thaiLocale;
    private static bool _localeResolved;
    private static CancellationTokenSource? _speechCts;

    public static async Task Speak(this IVoiced _, string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        if (!_localeResolved)
        {
            IEnumerable<Locale> locales = await TextToSpeech.Default.GetLocalesAsync();
            _thaiLocale = locales.FirstOrDefault(x => x.Language == "th");
            _localeResolved = true;
        }

        _speechCts?.Cancel();
        _speechCts?.Dispose();
        CancellationTokenSource cts = _speechCts = new CancellationTokenSource();

        try
        {
            await TextToSpeech.Default.SpeakAsync(text, new SpeechOptions
            {
                Locale = _thaiLocale,
                Volume = 1.0f
            }, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // replaced by a newer tap
        }
    }

    public static async Task Pronounce(this IVoiced voiced, object? item)
    {
        if (item is null)
            return;

        bool preferRecorded = Preferences.Get("PreferRecorded", false);
        bool hasRecording = item is IHaveRecording rec && rec.Media?.Length > 0;
        bool hasMediaUrl = item is IHaveMediaUrl withUrl && !string.IsNullOrWhiteSpace(withUrl.MediaUrl);
        // Sounds (IPA phonemes) are recording/URL only — never TTS.
        bool canTts = item is IAudible && item is not ISound;

        if (item is ISound)
        {
            if (hasRecording) { await voiced.PlayAudioAsync((IHaveRecording)item); return; }
            if (hasMediaUrl) { await voiced.PlayFromUrlAsync(((IHaveMediaUrl)item).MediaUrl!); }
            return;
        }

        if (preferRecorded)
        {
            if (hasRecording) { await voiced.PlayAudioAsync((IHaveRecording)item); return; }
            if (hasMediaUrl) { await voiced.PlayFromUrlAsync(((IHaveMediaUrl)item).MediaUrl!); return; }
            if (canTts) { await voiced.Speak(ToSpeakText(item)); }
        }
        else if (canTts) { await voiced.Speak(ToSpeakText(item)); }
        else {
            if (hasRecording) { await voiced.PlayAudioAsync((IHaveRecording)item); }
            else if (hasMediaUrl) { await voiced.PlayFromUrlAsync(((IHaveMediaUrl)item).MediaUrl!); }
        }
    }

    private static string ToSpeakText(object item) =>
        item switch
        {
            IAudible audible => audible.Audible,
            _ => string.Empty,
        };
}
