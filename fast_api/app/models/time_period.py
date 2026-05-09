from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import ForeignKey, Integer, String, UniqueConstraint
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.word import Word


class TimePeriod(Base):
    """Generic Thai time-label table; currently holds DayOfWeek and TimeOfDay rows."""

    __tablename__ = "TimePeriod"
    __table_args__ = (
        UniqueConstraint("kind", "ordinal", name="uq_thai_time_period_kind_ord"),
        {"schema": "thai", "extend_existing": True},
    )

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    kind: Mapped[str] = mapped_column(String(32), nullable=False)
    ordinal: Mapped[int] = mapped_column(Integer, nullable=False)
    word_id: Mapped[int] = mapped_column(
        Integer,
        ForeignKey("Word.id"),
        nullable=False,
    )

    word_rel: Mapped[Word] = relationship(
        "Word",
        foreign_keys=[word_id],
    )

    def __repr__(self) -> str:
        return f"TimePeriod(kind={self.kind!r}, ord={self.ordinal})"
