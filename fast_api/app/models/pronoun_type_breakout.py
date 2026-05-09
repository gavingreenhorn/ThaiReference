from __future__ import annotations

from sqlalchemy import ForeignKey, Integer
from sqlalchemy.orm import Mapped, mapped_column

from app.models.base import Base


class PronounTypeBreakout(Base):
    __tablename__ = "PronounTypeBreakout"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    pronoun_id: Mapped[int | None] = mapped_column(
        Integer, ForeignKey("Pronoun.id"), nullable=True
    )
    type_id: Mapped[int | None] = mapped_column(
        Integer, ForeignKey("PronounType.id"), nullable=True
    )

    def __repr__(self) -> str:
        return f"PronounTypeBreakout(pronoun_id={self.pronoun_id}, type_id={self.type_id})"
