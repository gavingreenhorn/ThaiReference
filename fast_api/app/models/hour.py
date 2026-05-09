from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import ForeignKey, Integer
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.word import Word


class Hour(Base):
    """Standalone hour entity (decoupled from TimePeriod). Ordinal IS the hour 0..23."""

    __tablename__ = "Hour"
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
        return f"Hour(ord={self.ordinal})"
