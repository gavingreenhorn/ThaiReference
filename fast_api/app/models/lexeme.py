from __future__ import annotations

from sqlalchemy import Integer, String, Text
from sqlalchemy.orm import Mapped, mapped_column

from app.models.base import Base


class Lexeme(Base):
    __tablename__ = "Lexeme"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    value: Mapped[str | None] = mapped_column(Text, nullable=True)
    part_of_speech: Mapped[str | None] = mapped_column(String(50), nullable=True)
    tags: Mapped[str | None] = mapped_column(String(64), nullable=True)

    def __repr__(self) -> str:
        return f"Lexeme({self.value or f'id={self.id}'})"
