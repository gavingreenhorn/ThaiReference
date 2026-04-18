from pydantic import BaseModel, ConfigDict


class TranscriptionDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    phonemic: str | None = None
    paiboon: str | None = None
    ipa: str | None = None


class SoundDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    id: int
    sound_type: str
    is_consonant_sound: bool
    is_initial_only: bool
    media_url: str | None = None
    media: bytes | None = None  # Kept for contract; API always null → client uses media_url or TTS
    transcription: TranscriptionDto


class WordDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    id: int
    value: str
    media: bytes | None = None  # Kept for contract; API always null → client uses media_url or TTS
    transcription: TranscriptionDto
    lexemes: list[str] = []


class VowelGraphemeDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    value: str
    is_closed: bool


class ConsonantDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    grapheme: str
    phoneme: str
    consonant_class: str
    ending: str
    media: bytes | None = None  # Kept for contract; API always null → client uses media_url or TTS
    transcription: TranscriptionDto
    initial_sound: SoundDto
    ending_sound: SoundDto | None = None
    sample_words: list[WordDto] = []


class VowelDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    is_diphtong: bool
    sound: SoundDto
    media_url: str | None = None
    graphemes: list[VowelGraphemeDto] = []
    sample_words: list[WordDto] = []


class MarkDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    grapheme: str
    tone_mark: bool
    media: bytes | None = None  # Kept for contract; API always null → client uses media_url or TTS
    words: list[WordDto] = []


class NumeralDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    grapheme: str
    name: WordDto


class PronounDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    name: WordDto
    person: int
    plurality: int
    formality: str
    gender: str
    types: list[str] = []


class PositionCellDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    row: int
    col: int
    tile_style: str | None = None
    label: str | None = None
    words: list[WordDto]
    thai_display: str
    english_display: str
