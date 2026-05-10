from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import ForeignKey, Integer, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.word import Word


class CelestialBody(Base):
    """Planet / luminary associated with a Thai weekday (ordinal 0..6 with DayOfWeek)."""

    __tablename__ = "CelestialBody"
    __table_args__ = {"schema": "thai", "extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    ordinal: Mapped[int] = mapped_column(Integer, nullable=False, unique=True)
    glyph: Mapped[str] = mapped_column(String(32), nullable=False)
    color_hex: Mapped[str] = mapped_column(String(16), nullable=False)
    word_id: Mapped[int] = mapped_column(
        Integer,
        ForeignKey("Word.id"),
        nullable=False,
    )

    word_rel: Mapped[Word] = relationship("Word", foreign_keys=[word_id])

    def __repr__(self) -> str:
        return f"CelestialBody(ord={self.ordinal})"
