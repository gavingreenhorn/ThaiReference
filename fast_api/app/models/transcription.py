from __future__ import annotations

from sqlalchemy import Integer, String, Text
from sqlalchemy.orm import Mapped, mapped_column

from app.models.base import Base


class Transcription(Base):
    __tablename__ = "Transcription"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    phonemic: Mapped[str | None] = mapped_column(String(25), nullable=True)
    paiboon: Mapped[str | None] = mapped_column(String(25), nullable=True)
    IPA: Mapped[str | None] = mapped_column(Text, nullable=True)

    def __repr__(self) -> str:
        label = self.phonemic or self.paiboon or self.IPA or f"id={self.id}"
        return f"Transcription({label})"
