from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import ForeignKey, Integer, String, Text
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.consonant_sample import ConsonantSample
    from app.models.sound import Sound
    from app.models.transcription import Transcription
    from app.models.word import Word


class Consonant(Base):
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

    transcription_rel: Mapped[Transcription | None] = relationship(
        "Transcription", foreign_keys=[transcription]
    )
    initial_sound_rel: Mapped[Sound | None] = relationship(
        "Sound", foreign_keys=[initial]
    )
    final_sound_rel: Mapped[Sound | None] = relationship(
        "Sound", foreign_keys=[final_sound_id]
    )
    acrophonic_rel: Mapped[Word | None] = relationship(
        "Word", foreign_keys=[acrophonic]
    )
    sample_links: Mapped[list[ConsonantSample]] = relationship(
        "ConsonantSample", back_populates="consonant_rel", foreign_keys="[ConsonantSample.consonant]"
    )

    def __repr__(self) -> str:
        return f"Consonant({self.grapheme} / {self.phoneme})"
