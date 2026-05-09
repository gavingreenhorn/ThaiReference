from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import Boolean, ForeignKey, Integer, Text
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.vowel import Vowel


class VowelGrapheme(Base):
    __tablename__ = "VowelGrapheme"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    vowel_id: Mapped[int | None] = mapped_column(Integer, ForeignKey("Vowel.id"), nullable=True)
    value: Mapped[str | None] = mapped_column(Text, nullable=True)
    is_closed: Mapped[bool | None] = mapped_column(Boolean, nullable=True)

    vowel_rel: Mapped[Vowel | None] = relationship(
        "Vowel", back_populates="graphemes", foreign_keys=[vowel_id]
    )

    def __repr__(self) -> str:
        return f"VowelGrapheme({self.value or self.id})"
