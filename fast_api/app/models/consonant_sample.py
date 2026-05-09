from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import ForeignKey, Integer
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.consonant import Consonant
    from app.models.word import Word


class ConsonantSample(Base):
    __tablename__ = "ConsonantSample"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    consonant: Mapped[int | None] = mapped_column(
        Integer, ForeignKey("Consonant.id"), nullable=True
    )
    word: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)

    consonant_rel: Mapped[Consonant | None] = relationship(
        "Consonant", back_populates="sample_links", foreign_keys=[consonant]
    )
    word_rel: Mapped[Word | None] = relationship(
        "Word", foreign_keys=[word]
    )

    def __repr__(self) -> str:
        try:
            c = self.consonant_rel.grapheme if self.consonant_rel is not None else self.consonant
        except Exception:
            c = self.consonant
        try:
            w = self.word_rel.value if self.word_rel is not None else self.word
        except Exception:
            w = self.word
        return f"ConsonantSample({c} + word {w})"
