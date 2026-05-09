from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import ForeignKey, Integer, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.word import Word
    from app.models.zodiac import Zodiac


class Month(Base):
    """Standalone month entity (decoupled from TimePeriod)."""

    __tablename__ = "Month"
    __table_args__ = {"schema": "thai", "extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    ordinal: Mapped[int] = mapped_column(Integer, nullable=False, unique=True)
    word_id: Mapped[int] = mapped_column(
        Integer,
        ForeignKey("Word.id"),
        nullable=False,
    )
    short_name: Mapped[str] = mapped_column(String(16), nullable=False)
    english_name: Mapped[str] = mapped_column(String(32), nullable=False)
    zodiac_id: Mapped[int] = mapped_column(
        Integer,
        ForeignKey("Zodiac.id"),
        nullable=False,
    )

    word_rel: Mapped[Word] = relationship(
        "Word", foreign_keys=[word_id]
    )
    zodiac_rel: Mapped[Zodiac] = relationship(
        "Zodiac", foreign_keys=[zodiac_id]
    )

    def __repr__(self) -> str:
        return f"Month(ord={self.ordinal}, {self.english_name})"
