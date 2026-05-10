from __future__ import annotations

from __future__ import annotations

from sqlalchemy import ForeignKey, Integer, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base


class CelestialBody(Base):
    """Planet / luminary associated with a Thai weekday (ordinal 0..6 with DayOfWeek)."""

    __tablename__ = "CelestialBody"
    __table_args__ = {"schema": "thai", "extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    ordinal: Mapped[int] = mapped_column(Integer, nullable=False, unique=True)
    glyph: Mapped[str] = mapped_column(String(32), nullable=False)
    color_hex: Mapped[str] = mapped_column(String(16), nullable=False)
    thai_name_word_id: Mapped[int] = mapped_column(
        Integer,
        ForeignKey("Word.id"),
        nullable=False,
    )
    english_name_word_id: Mapped[int] = mapped_column(
        Integer,
        ForeignKey("Word.id"),
        nullable=False,
    )

    thai_name_word_rel: Mapped["Word"] = relationship(
        "Word", foreign_keys=[thai_name_word_id]
    )
    english_name_word_rel: Mapped["Word"] = relationship(
        "Word", foreign_keys=[english_name_word_id]
    )

    def __repr__(self) -> str:
        return f"CelestialBody(ord={self.ordinal})"
