using ThaiReference.Models.Interfaces;

namespace ThaiReference.Models.BaseModels;

public class HomophoneGroup : IHomophoneGroup
{
    public ITranscription Transcription { get; set; } = null!;
    public IEnumerable<IWord> Words { get; set; } = [];
}
