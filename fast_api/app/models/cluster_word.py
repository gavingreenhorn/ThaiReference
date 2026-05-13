from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import ForeignKey, Integer, UniqueConstraint
from sqlalchemy.orm import Mapped, mapped_column, relationship

from app.models.base import Base

if TYPE_CHECKING:
    from app.models.cluster import Cluster
    from app.models.word import Word


class ClusterWord(Base):
    """Sample dictionary word illustrating a cluster (max ~5 per cluster); order by ``id`` ascending."""

    __tablename__ = "ClusterWord"
    __table_args__ = (
        UniqueConstraint("cluster_id", "word_id", name="uq_thai_cluster_word_pair"),
        {"extend_existing": True},
    )

    id: Mapped[int] = mapped_column(Integer, primary_key=True, autoincrement=True)
    cluster_id: Mapped[int] = mapped_column(
        Integer, ForeignKey("Cluster.id", ondelete="CASCADE"), nullable=False
    )
    word_id: Mapped[int] = mapped_column(Integer, ForeignKey("Word.id"), nullable=False)

    cluster_rel: Mapped["Cluster"] = relationship("Cluster", back_populates="sample_links")
    word_rel: Mapped["Word"] = relationship("Word", foreign_keys=[word_id])

    def __repr__(self) -> str:
        return f"ClusterWord(cluster={self.cluster_id}, word={self.word_id})"
