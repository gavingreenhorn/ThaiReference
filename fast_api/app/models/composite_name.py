from __future__ import annotations

from sqlalchemy import ForeignKey, Integer
from sqlalchemy.orm import Mapped, mapped_column

from app.models.base import Base


class CompositeName(Base):
    __tablename__ = "CompositeName"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    word1: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)
    word2: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)
    word3: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)
    word4: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)
    word5: Mapped[int | None] = mapped_column(Integer, ForeignKey("Word.id"), nullable=True)

    def __repr__(self) -> str:
        return f"CompositeName(id={self.id}, words {self.word1}-{self.word5})"
