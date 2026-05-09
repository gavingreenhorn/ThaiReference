from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import ForeignKey, Integer
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.formality import Formality
    from app.models.gender import Gender
    from app.models.plurality import Plurality
    from app.models.word import Word


class Pronoun(Base):
    __tablename__ = "Pronoun"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    word: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)
    gender: Mapped[int | None] = mapped_column(Integer, ForeignKey("Gender.id"), nullable=True)
    formality: Mapped[int | None] = mapped_column(
        Integer, ForeignKey("Formality.id"), nullable=True
    )
    plurality: Mapped[int | None] = mapped_column(
        Integer, ForeignKey("Plurality.id"), nullable=True
    )
    grammatical_person: Mapped[int | None] = mapped_column(Integer, nullable=True)

    word_rel: Mapped[Word | None] = relationship(
        "Word", foreign_keys=[word]
    )
    gender_rel: Mapped[Gender | None] = relationship(
        "Gender", foreign_keys=[gender]
    )
    formality_rel: Mapped[Formality | None] = relationship(
        "Formality", foreign_keys=[formality]
    )
    plurality_rel: Mapped[Plurality | None] = relationship(
        "Plurality", foreign_keys=[plurality]
    )

    def __repr__(self) -> str:
        try:
            w = self.word_rel.value if self.word_rel is not None else self.word
        except Exception:
            w = self.word
        return f"Pronoun(word={w})"
