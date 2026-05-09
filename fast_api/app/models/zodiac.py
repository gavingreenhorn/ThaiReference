from __future__ import annotations

from sqlalchemy import Integer, String, Text
from sqlalchemy.orm import Mapped, mapped_column

from app.models.base import Base


class Zodiac(Base):
    """Zodiac sign metadata; element-to-color mapping stays client-side."""

    __tablename__ = "Zodiac"
    __table_args__ = {"schema": "thai", "extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    ordinal: Mapped[int] = mapped_column(Integer, nullable=False, unique=True)
    english_name: Mapped[str] = mapped_column(String(32), nullable=False)
    sanskrit_name: Mapped[str] = mapped_column(String(32), nullable=False)
    sanskrit_lexeme: Mapped[str] = mapped_column(String(64), nullable=False)
    element: Mapped[str] = mapped_column(String(16), nullable=False)
    image_url: Mapped[str | None] = mapped_column(Text, nullable=True)

    def __repr__(self) -> str:
        return f"Zodiac(ord={self.ordinal}, {self.english_name})"
