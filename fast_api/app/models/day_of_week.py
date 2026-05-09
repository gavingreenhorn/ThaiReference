from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import ForeignKey, Integer
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.word import Word


class DayOfWeek(Base):
    """Thai weekday label (decoupled from TimePeriod). Ordinal is stable 0..6."""

    __tablename__ = "DayOfWeek"
    __table_args__ = {"schema": "thai", "extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    ordinal: Mapped[int] = mapped_column(Integer, nullable=False, unique=True)
    word_id: Mapped[int] = mapped_column(
        Integer,
        ForeignKey("Word.id"),
        nullable=False,
    )

    word_rel: Mapped[Word] = relationship(
        "Word", foreign_keys=[word_id]
    )

    def __repr__(self) -> str:
        return f"DayOfWeek(ord={self.ordinal})"
