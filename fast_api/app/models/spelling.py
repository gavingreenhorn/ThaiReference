from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import Integer, String, UniqueConstraint
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.word import Word


class Spelling(Base):
    __tablename__ = "Spelling"
    __table_args__ = (
        UniqueConstraint("value", name="uq_thai_spelling_value"),
        {"extend_existing": True},
    )

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    value: Mapped[str] = mapped_column(String(25), nullable=False)

    words: Mapped[list["Word"]] = relationship(
        "Word", back_populates="spelling_rel", foreign_keys="Word.spelling_id"
    )

    def __repr__(self) -> str:
        return f"Spelling({self.value!r})"
