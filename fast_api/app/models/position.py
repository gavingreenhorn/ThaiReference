from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import Integer, String, Text, UniqueConstraint
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.position_word import PositionWord


class Position(Base):
    __tablename__ = "Position"
    __table_args__ = (
        UniqueConstraint("row_index", "col_index", name="uq_thai_position_row_col"),
        {"schema": "thai", "extend_existing": True},
    )

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    row_index: Mapped[int] = mapped_column(Integer, nullable=False)
    col_index: Mapped[int] = mapped_column(Integer, nullable=False)
    name: Mapped[str | None] = mapped_column(Text, nullable=True)
    tile_style: Mapped[str | None] = mapped_column(String(32), nullable=True)

    position_words: Mapped[list[PositionWord]] = relationship(
        "PositionWord",
        foreign_keys="[PositionWord.position_id]",
        back_populates="position_rel",
        order_by="PositionWord.word_order",
    )

    def __repr__(self) -> str:
        return f"Position(r={self.row_index}, c={self.col_index})"
