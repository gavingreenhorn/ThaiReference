from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import ForeignKey, Integer, Text
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.word import Word


class Numeral(Base):
    __tablename__ = "Numeral"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    word: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)
    grapheme: Mapped[str | None] = mapped_column(Text, nullable=True)

    word_rel: Mapped[Word | None] = relationship(
        "Word", foreign_keys=[word]
    )

    def __repr__(self) -> str:
        return f"Numeral({self.grapheme or self.id})"
