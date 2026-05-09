from __future__ import annotations

from sqlalchemy import Integer, String
from sqlalchemy.orm import Mapped, mapped_column

from app.models.base import Base


class SoundType(Base):
    __tablename__ = "SoundType"
    __table_args__ = {"extend_existing": True}

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    name: Mapped[str] = mapped_column(String(15), nullable=False)

    def __repr__(self) -> str:
        return f"SoundType({self.name})"
