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
    transcription: TranscriptionDto
    media_url: str | None = None


class WordDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    id: int
    value: str
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
    transcription: TranscriptionDto
    initial_sound: SoundDto
    ending_sound: SoundDto | None = None
    media_url: str | None = None
    sample_words: list[WordDto] = []


class VowelDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    is_diphtong: bool
    sound: SoundDto
    graphemes: list[VowelGraphemeDto] = []
    media_url: str | None = None
    sample_words: list[WordDto] = []


class MarkDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    grapheme: str
    tone_mark: bool
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
    ordinal: int
    row: int
    col: int
    tile_style: str | None = None
    label: str | None = None
    words: list[WordDto]


class DayOfWeekDto(BaseModel):
    """One Thai weekday name with stable ordering (ordinal 0..6)."""

    model_config = ConfigDict(populate_by_name=True)
    ordinal: int
    word: WordDto
    deity: str | None = None
    image_url: str | None = None


class TimeOfDayDto(BaseModel):
    """One of the named Thai time-of-day bands (morning, evening, ...)."""

    model_config = ConfigDict(populate_by_name=True)
    ordinal: int
    word: WordDto


class HourDto(BaseModel):
    """A single hour of the day. ``ordinal`` IS the hour (0..23)."""

    model_config = ConfigDict(populate_by_name=True)
    ordinal: int
    word: WordDto


class ZodiacDto(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    ordinal: int
    english_name: str
    sanskrit_name: str
    sanskrit_lexeme: str
    element: str
    image_url: str | None = None


class MonthDto(BaseModel):
    """Standalone month entity with its underlying Word and zodiac association."""

    model_config = ConfigDict(populate_by_name=True)
    ordinal: int
    word: WordDto
    short_name: str
    english_name: str
    zodiac: ZodiacDto
