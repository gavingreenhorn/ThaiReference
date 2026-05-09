from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import ForeignKey, Integer, UniqueConstraint
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.position import Position
    from app.models.word import Word


class PositionWord(Base):
    __tablename__ = "PositionWord"
    __table_args__ = (
        UniqueConstraint("position_id", "word_order", name="uq_thai_position_word_order"),
        {"schema": "thai", "extend_existing": True},
    )

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    position_id: Mapped[int] = mapped_column(
        Integer,
        ForeignKey("Position.id", ondelete="CASCADE"),
        nullable=False,
    )
    word_id: Mapped[int] = mapped_column(
        Integer,
        ForeignKey("Word.id"),
        nullable=False,
    )
    word_order: Mapped[int] = mapped_column(Integer, nullable=False)

    position_rel: Mapped[Position] = relationship(
        "Position",
        foreign_keys=[position_id],
        back_populates="position_words",
    )
    word_rel: Mapped[Word] = relationship(
        "Word",
        foreign_keys=[word_id],
        lazy="raise"
    )

    def __repr__(self) -> str:
        return f"PositionWord(position={self.position_id}, order={self.word_order})"
