from sqlmodel import Column, Field, Relationship, String, BigInteger, UniqueConstraint
from pydantic import HttpUrl

from app.shortcuts import ThaiModel, one_to_one


class Transcription(ThaiModel, table=True):
    id: int | None = Field(default=None, primary_key=True, sa_type=BigInteger)
    phonemic: str | None
    paiboon: str | None
    ipa: str | None

    __table_args__ = (
        UniqueConstraint("phonemic", "paiboon", "ipa"),
    )

    word: 'Word' = one_to_one(back_populates="transcription")

    def __str__(self) -> str:
        return f"Transcription(id={self.id}, phonemic={self.phonemic}, paiboon={self.paiboon}, ipa={self.ipa})"
    

class PartOfSpeech(ThaiModel, table=True):
    __tablename__ = "part_of_speech"

    id: int | None = Field(default=None, primary_key=True, sa_type=BigInteger)
    name: str

    def __str__(self) -> str:
        return f"PartOfSpeech(id={self.id}, name={self.name})"


class WordLexeme(ThaiModel, table=True):

    id: int | None = Field(default=None, primary_key=True, sa_type=BigInteger)
    word_id: int  = Field(foreign_key="thai.word.id", sa_type=BigInteger)
    lexeme_id: int = Field(foreign_key="thai.lexeme.id", sa_type=BigInteger)

    __table_args__ = (
        UniqueConstraint("word_id", "lexeme_id"),
    )

    def __str__(self) -> str:
        return f"WordLexeme(id={self.id}, word_id={self.word_id}, lexeme_id={self.lexeme_id})"
    

class ConsonantSample(ThaiModel, table=True):
    id: int | None = Field(default=None, primary_key=True, sa_type=BigInteger)
    consonant_id: int  = Field(foreign_key="thai.consonant.id", sa_type=BigInteger)
    word_id: int = Field(foreign_key="thai.word.id", sa_type=BigInteger)

    __table_args__ = (
        UniqueConstraint("consonant_id", "word_id"),
    )

    def __str__(self) -> str:
        return f"ConsonantSample(id={self.id}, consonant_id={self.consonant_id}, word_id={self.word_id})"


class Lexeme(ThaiModel, table=True):
    id: int | None = Field(default=None, primary_key=True, sa_type=BigInteger)
    value: str
    part_of_speech_id: int | None = Field(default=None, foreign_key="thai.part_of_speech.id", sa_type=BigInteger)

    words: list['Word'] = Relationship(back_populates="lexemes", link_model=WordLexeme)
    part_of_speech: PartOfSpeech | None = Relationship()

    def __str__(self) -> str:
        return f"Lexeme(id={self.id}, value={self.value})"


class Word(ThaiModel, table=True):
    id: int | None = Field(default=None, primary_key=True, sa_type=BigInteger)
    value: str
    media_url: HttpUrl | None = Field(default=None, sa_column=Column(String(2048), nullable=True))
    transcription_id: int | None = Field(default=None, unique=True, foreign_key="thai.transcription.id", sa_type=BigInteger)

    lexemes: list[Lexeme] = Relationship(back_populates="words", link_model=WordLexeme)

    def __str__(self) -> str:
        return f"Word(id={self.id}, value={self.value}, media_url={self.media_url})"


class Sound(ThaiModel, table=True):
    id: int | None = Field(default=None, primary_key=True, sa_type=BigInteger)
    sound_type: str
    is_consonant_sound: bool
    is_initial_only: bool
    media_url: HttpUrl | None = Field(default=None, sa_column=Column(String(2048), nullable=True))
    transcription_id: int | None = Field(default=None, unique=True, foreign_key="thai.transcription.id", sa_type=BigInteger)
    
    transcription: Transcription | None = one_to_one()

    
class Consonant(ThaiModel, table=True):
    id: int | None = Field(default=None, primary_key=True, sa_type=BigInteger)
    transcription_id: int | None = Field(default=None, unique=True, foreign_key="thai.transcription.id", sa_type=BigInteger)
    acrophonic_word_id: int | None = Field(default=None, foreign_key="thai.word.id", sa_type=BigInteger)
    grapheme: str
    phoneme: str
    media_url: HttpUrl | None = Field(default=None, sa_column=Column(String(2048), nullable=True))
    initial_sound_id: int = Field(foreign_key="thai.sound.id", sa_type=BigInteger)
    ending_sound_id: int | None = Field(default=None, foreign_key="thai.sound.id", sa_type=BigInteger)

    initial_sound: Sound = one_to_one()
    ending_sound: Sound | None = one_to_one()   
    acrophonic: Word = one_to_one()
    sample_words: list[Word] = Relationship(link_model=ConsonantSample)
    

    def __str__(self) -> str:
        return f"Consonant(id={self.id}, grapheme={self.grapheme}, phoneme={self.phoneme})"

