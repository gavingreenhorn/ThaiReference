from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import ForeignKey, Integer, String, Text
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.composite_name import CompositeName


class ToneMark(Base):
    __tablename__ = "ToneMark"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    mark: Mapped[str] = mapped_column(String(2), nullable=False)
    name: Mapped[int | None] = mapped_column(
        Integer, ForeignKey("CompositeName.id"), nullable=True
    )
    mediapath: Mapped[str | None] = mapped_column(Text, nullable=True)

    name_rel: Mapped[CompositeName | None] = relationship(
        "CompositeName", foreign_keys=[name]
    )

    def __repr__(self) -> str:
        return f"ToneMark({self.mark})"
