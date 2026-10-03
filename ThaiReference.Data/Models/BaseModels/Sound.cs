using ThaiReference.Contracts;
using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models.BaseModels;

public class Sound : ISound
{
    public string Value { get; set; } = "";
    public string SoundType { get; set; } = "";
    public bool IsConsonantSound { get; set; }
    public bool IsInitialOnly { get; set; }
    public string? MediaUrl { get; set; }
    public byte[] Media { get; set; } = [];

    byte[]? IHaveRecording.Media => Media;
    string? IHaveMediaUrl.MediaUrl => MediaUrl;
}
