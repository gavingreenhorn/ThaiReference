from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import Integer, String, UniqueConstraint
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.cluster_word import ClusterWord


class Cluster(Base):
    """One teachable consonant cluster: ``thai.Cluster.type`` + ``thai.Cluster.value``."""

    __tablename__ = "Cluster"
    __table_args__ = (
        UniqueConstraint("type", "value", name="uq_thai_cluster_type_value"),
        {"extend_existing": True},
    )

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    type: Mapped[str] = mapped_column("type", String(64), nullable=False)  # noqa: A003
    value: Mapped[str] = mapped_column("value", String(64), nullable=False)

    sample_links: Mapped[list["ClusterWord"]] = relationship(
        "ClusterWord", back_populates="cluster_rel"
    )

    def __repr__(self) -> str:
        return f"Cluster({self.type!r}, {self.value!r})"
