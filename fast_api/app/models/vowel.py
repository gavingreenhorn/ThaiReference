from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import Boolean, ForeignKey, Integer, Text
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.sound import Sound
    from app.models.vowel_grapheme import VowelGrapheme
    from app.models.vowel_sample import VowelSample


class Vowel(Base):
    __tablename__ = "Vowel"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    sound_id: Mapped[int | None] = mapped_column(Integer, ForeignKey("Sound.id"), nullable=True)
    is_diphtong: Mapped[bool | None] = mapped_column(Boolean, nullable=True)
    mediapath: Mapped[str | None] = mapped_column(Text, nullable=True)

    sound_rel: Mapped[Sound | None] = relationship(
        "Sound", foreign_keys=[sound_id]
    )
    graphemes: Mapped[list[VowelGrapheme]] = relationship(
        "VowelGrapheme", back_populates="vowel_rel", foreign_keys="[VowelGrapheme.vowel_id]"
    )
    sample_links: Mapped[list[VowelSample]] = relationship(
        "VowelSample", back_populates="vowel_rel", foreign_keys="[VowelSample.vowel]"
    )

    def __repr__(self) -> str:
        return f"Vowel(id={self.id}, sound_id={self.sound_id})"
