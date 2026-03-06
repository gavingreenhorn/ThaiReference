"""
ORM models mapped to the actual tables in thai_sqlite.db.
Used by ThaiRepository to load data; db-agnostic (same models can target Postgres later).
"""
from __future__ import annotations

from sqlalchemy import (
    Boolean,
    ForeignKey,
    Integer,
    LargeBinary,
    String,
    Text,
)
from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column, relationship


class Base(DeclarativeBase):
    pass


# ---------------------------------------------------------------------------
# Lookup / reference tables
# ---------------------------------------------------------------------------


class TranscriptionOrm(Base):
    __tablename__ = "Transcription"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    phonemic: Mapped[str | None] = mapped_column(String(25), nullable=True)
    paiboon: Mapped[str | None] = mapped_column(String(25), nullable=True)
    IPA: Mapped[str | None] = mapped_column(Text, nullable=True)


class SoundTypeOrm(Base):
    __tablename__ = "SoundType"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    name: Mapped[str] = mapped_column(String(15), nullable=False)


class LexemeOrm(Base):
    __tablename__ = "Lexeme"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    value: Mapped[str | None] = mapped_column(Text, nullable=True)


class ClassOrm(Base):
    __tablename__ = "Class"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    name: Mapped[str] = mapped_column(String(15), nullable=False)


class EndingOrm(Base):
    __tablename__ = "Ending"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    name: Mapped[str] = mapped_column(String(15), nullable=False)


class GenderOrm(Base):
    __tablename__ = "Gender"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    name: Mapped[str | None] = mapped_column(Text, nullable=True)


class FormalityOrm(Base):
    __tablename__ = "Formality"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    name: Mapped[str | None] = mapped_column(Text, nullable=True)


class PluralityOrm(Base):
    __tablename__ = "Plurality"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    name: Mapped[str | None] = mapped_column(Text, nullable=True)


class PronounTypeOrm(Base):
    __tablename__ = "PronounType"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    name: Mapped[str | None] = mapped_column(Text, nullable=True)


# ---------------------------------------------------------------------------
# Core entities
# ---------------------------------------------------------------------------


class WordOrm(Base):
    __tablename__ = "Word"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    transcription: Mapped[int | None] = mapped_column(
        Integer, ForeignKey("Transcription.id"), nullable=True
    )
    value: Mapped[str | None] = mapped_column(String(25), nullable=True)
    media: Mapped[bytes | None] = mapped_column(LargeBinary, nullable=True)

    transcription_rel: Mapped[TranscriptionOrm | None] = relationship(
        "TranscriptionOrm", foreign_keys=[transcription]
    )
    word_lexemes: Mapped[list[WordLexemeOrm]] = relationship(
        "WordLexemeOrm", foreign_keys="[WordLexemeOrm.word]", back_populates="word_rel"
    )


class WordLexemeOrm(Base):
    __tablename__ = "WordLexeme"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    word: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)
    lexeme: Mapped[int | None] = mapped_column(Integer, ForeignKey("Lexeme.id"), nullable=True)

    word_rel: Mapped[WordOrm | None] = relationship(
        "WordOrm", foreign_keys=[word], back_populates="word_lexemes"
    )
    lexeme_rel: Mapped[LexemeOrm | None] = relationship(
        "LexemeOrm", foreign_keys=[lexeme]
    )


class SoundOrm(Base):
    __tablename__ = "Sound"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    transcription: Mapped[int | None] = mapped_column(
        Integer, ForeignKey("Transcription.id"), nullable=True
    )
    mediapath: Mapped[str | None] = mapped_column(Text, nullable=True)
    initial_only: Mapped[bool] = mapped_column(Boolean, nullable=False, default=False)
    consonant_sound: Mapped[int | None] = mapped_column(Integer, nullable=True)
    sound_type: Mapped[int | None] = mapped_column(
        Integer, ForeignKey("SoundType.id"), nullable=True
    )
    media: Mapped[bytes | None] = mapped_column(LargeBinary, nullable=True)

    transcription_rel: Mapped[TranscriptionOrm | None] = relationship(
        "TranscriptionOrm", foreign_keys=[transcription]
    )
    sound_type_rel: Mapped[SoundTypeOrm | None] = relationship(
        "SoundTypeOrm", foreign_keys=[sound_type]
    )


class ConsonantOrm(Base):
    __tablename__ = "Consonant"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    grapheme: Mapped[str] = mapped_column(String(1), nullable=False)
    phoneme: Mapped[str] = mapped_column(String(2), nullable=False)
    soundtype: Mapped[str] = mapped_column(String(10), nullable=False)
    class_: Mapped[str] = mapped_column("class", String(10), nullable=False)
    ending: Mapped[str] = mapped_column(String(5), nullable=False)
    transcription: Mapped[int | None] = mapped_column(
        Integer, ForeignKey("Transcription.id"), nullable=True
    )
    acrophonic: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)
    mediapath: Mapped[str | None] = mapped_column(Text, nullable=True)
    initial: Mapped[int | None] = mapped_column(Integer, ForeignKey("Sound.id"), nullable=True)
    final_sound_id: Mapped[int | None] = mapped_column(
        "final", Integer, ForeignKey("Sound.id"), nullable=True
    )
    media: Mapped[bytes | None] = mapped_column(LargeBinary, nullable=True)

    transcription_rel: Mapped[TranscriptionOrm | None] = relationship(
        "TranscriptionOrm", foreign_keys=[transcription]
    )
    initial_sound_rel: Mapped[SoundOrm | None] = relationship(
        "SoundOrm", foreign_keys=[initial]
    )
    final_sound_rel: Mapped[SoundOrm | None] = relationship(
        "SoundOrm", foreign_keys=[final_sound_id]
    )
    acrophonic_rel: Mapped[WordOrm | None] = relationship(
        "WordOrm", foreign_keys=[acrophonic]
    )
    sample_links: Mapped[list[ConsonantSampleOrm]] = relationship(
        "ConsonantSampleOrm", back_populates="consonant_rel", foreign_keys="[ConsonantSampleOrm.consonant]"
    )


class ConsonantSampleOrm(Base):
    __tablename__ = "ConsonantSample"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    consonant: Mapped[int | None] = mapped_column(
        Integer, ForeignKey("Consonant.id"), nullable=True
    )
    word: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)

    consonant_rel: Mapped[ConsonantOrm | None] = relationship(
        "ConsonantOrm", back_populates="sample_links", foreign_keys=[consonant]
    )
    word_rel: Mapped[WordOrm | None] = relationship(
        "WordOrm", foreign_keys=[word]
    )


class VowelGraphemeOrm(Base):
    __tablename__ = "VowelGrapheme"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    vowel_id: Mapped[int | None] = mapped_column(Integer, ForeignKey("Vowel.id"), nullable=True)
    value: Mapped[str | None] = mapped_column(Text, nullable=True)
    is_closed: Mapped[bool | None] = mapped_column(Boolean, nullable=True)


class VowelSampleOrm(Base):
    __tablename__ = "VowelSample"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    vowel: Mapped[int | None] = mapped_column(Integer, ForeignKey("Vowel.id"), nullable=True)
    word: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)

    vowel_rel: Mapped[VowelOrm | None] = relationship(
        "VowelOrm", back_populates="sample_links", foreign_keys=[vowel]
    )
    word_rel: Mapped[WordOrm | None] = relationship(
        "WordOrm", foreign_keys=[word]
    )


class VowelOrm(Base):
    __tablename__ = "Vowel"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    sound_id: Mapped[int | None] = mapped_column(Integer, ForeignKey("Sound.id"), nullable=True)
    is_diphtong: Mapped[bool | None] = mapped_column(Boolean, nullable=True)

    sound_rel: Mapped[SoundOrm | None] = relationship(
        "SoundOrm", foreign_keys=[sound_id]
    )
    graphemes: Mapped[list[VowelGraphemeOrm]] = relationship(
        "VowelGraphemeOrm", back_populates="vowel_rel", foreign_keys="[VowelGraphemeOrm.vowel_id]"
    )
    sample_links: Mapped[list[VowelSampleOrm]] = relationship(
        "VowelSampleOrm", back_populates="vowel_rel", foreign_keys="[VowelSampleOrm.vowel]"
    )


VowelGraphemeOrm.vowel_rel = relationship(
    "VowelOrm", back_populates="graphemes", foreign_keys="[VowelGraphemeOrm.vowel_id]"
)


class NumeralOrm(Base):
    __tablename__ = "Numeral"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    word: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)
    grapheme: Mapped[str | None] = mapped_column(Text, nullable=True)

    word_rel: Mapped[WordOrm | None] = relationship(
        "WordOrm", foreign_keys=[word]
    )


class PronounOrm(Base):
    __tablename__ = "Pronoun"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    word: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)
    gender: Mapped[int | None] = mapped_column(Integer, ForeignKey("Gender.id"), nullable=True)
    formality: Mapped[int | None] = mapped_column(
        Integer, ForeignKey("Formality.id"), nullable=True
    )
    plurality: Mapped[int | None] = mapped_column(
        Integer, ForeignKey("Plurality.id"), nullable=True
    )
    grammatical_person: Mapped[int | None] = mapped_column(Integer, nullable=True)

    word_rel: Mapped[WordOrm | None] = relationship(
        "WordOrm", foreign_keys=[word]
    )
    gender_rel: Mapped[GenderOrm | None] = relationship(
        "GenderOrm", foreign_keys=[gender]
    )
    formality_rel: Mapped[FormalityOrm | None] = relationship(
        "FormalityOrm", foreign_keys=[formality]
    )
    plurality_rel: Mapped[PluralityOrm | None] = relationship(
        "PluralityOrm", foreign_keys=[plurality]
    )


class PronounTypeBreakoutOrm(Base):
    __tablename__ = "PronounTypeBreakout"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    pronoun_id: Mapped[int | None] = mapped_column(
        Integer, ForeignKey("Pronoun.id"), nullable=True
    )
    type_id: Mapped[int | None] = mapped_column(
        Integer, ForeignKey("PronounType.id"), nullable=True
    )


class SpecialSymbolOrm(Base):
    __tablename__ = "SpecialSymbol"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    grapheme: Mapped[str | None] = mapped_column(Text, nullable=True)
    word_id: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)

    word_rel: Mapped[WordOrm | None] = relationship(
        "WordOrm", foreign_keys=[word_id]
    )


class CompositeNameOrm(Base):
    __tablename__ = "CompositeName"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    word1: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)
    word2: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)
    word3: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)
    word4: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)
    word5: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)
    media: Mapped[bytes | None] = mapped_column(LargeBinary, nullable=True)


class ToneMarkOrm(Base):
    __tablename__ = "ToneMark"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    mark: Mapped[str] = mapped_column(String(2), nullable=False)
    name: Mapped[int | None] = mapped_column(
        Integer, ForeignKey("CompositeName.id"), nullable=True
    )
    mediapath: Mapped[str | None] = mapped_column(Text, nullable=True)

    name_rel: Mapped[CompositeNameOrm | None] = relationship(
        "CompositeNameOrm", foreign_keys=[name]
    )
