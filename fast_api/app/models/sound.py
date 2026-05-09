from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import Boolean, ForeignKey, Integer, Text
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.sound_type import SoundType
    from app.models.transcription import Transcription


class Sound(Base):
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

    transcription_rel: Mapped[Transcription | None] = relationship(
        "Transcription", foreign_keys=[transcription]
    )
    sound_type_rel: Mapped[SoundType | None] = relationship(
        "SoundType", foreign_keys=[sound_type]
    )

    def __repr__(self) -> str:
        return f"Sound(id={self.id}, {self.mediapath or 'no path'})"
