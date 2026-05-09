from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import ForeignKey, Integer
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.lexeme import Lexeme
    from app.models.word import Word


class WordLexeme(Base):
    __tablename__ = "WordLexeme"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    word: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)
    lexeme: Mapped[int | None] = mapped_column(Integer, ForeignKey("Lexeme.id"), nullable=True)

    word_rel: Mapped[Word | None] = relationship(
        "Word", foreign_keys=[word], back_populates="word_lexemes"
    )
    lexeme_rel: Mapped[Lexeme | None] = relationship(
        "Lexeme", foreign_keys=[lexeme]
    )

    def __repr__(self) -> str:
        try:
            w = self.word_rel.value if self.word_rel is not None else f"word_id={self.word}"
        except Exception:
            w = f"word_id={self.word}"
        try:
            lex = self.lexeme_rel.value if self.lexeme_rel is not None else f"lexeme_id={self.lexeme}"
        except Exception:
            lex = f"lexeme_id={self.lexeme}"
        return f"WordLexeme({w} \u2194 {lex})"
