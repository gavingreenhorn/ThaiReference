from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import ForeignKey, Integer
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.vowel import Vowel
    from app.models.word import Word


class VowelSample(Base):
    __tablename__ = "VowelSample"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    vowel: Mapped[int | None] = mapped_column(Integer, ForeignKey("Vowel.id"), nullable=True)
    word: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)

    vowel_rel: Mapped[Vowel | None] = relationship(
        "Vowel", back_populates="sample_links", foreign_keys=[vowel]
    )
    word_rel: Mapped[Word | None] = relationship(
        "Word", foreign_keys=[word]
    )

    def __repr__(self) -> str:
        try:
            w = self.word_rel.value if self.word_rel is not None else self.word
        except Exception:
            w = self.word
        return f"VowelSample(vowel={self.vowel}, word {w})"
