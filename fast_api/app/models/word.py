from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import ForeignKey, Integer, String, UniqueConstraint
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.transcription import Transcription
    from app.models.word_lexeme import WordLexeme


class Word(Base):
    __tablename__ = "Word"
    __table_args__ = (
        UniqueConstraint("value", name="uq_thai_word_value"),
        {"extend_existing": True},
    )

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    transcription: Mapped[int | None] = mapped_column(
        Integer, ForeignKey("Transcription.id"), nullable=True
    )
    value: Mapped[str | None] = mapped_column(String(25), nullable=True)

    transcription_rel: Mapped[Transcription | None] = relationship(
        "Transcription", foreign_keys=[transcription]
    )
    word_lexemes: Mapped[list[WordLexeme]] = relationship(
        "WordLexeme", foreign_keys="[WordLexeme.word]", back_populates="word_rel"
    )

    def __repr__(self) -> str:
        return f"Word({self.value or f'id={self.id}'})"
