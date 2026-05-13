"""add thai Cluster / ClusterWord (subtype, no sort_order)

Revision ID: c2d3e4f5a6b7
Revises: a1b2c3d4e5f7
Create Date: 2026-05-10
"""

from __future__ import annotations

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "c2d3e4f5a6b7"
down_revision: Union[str, Sequence[str], None] = "a1b2c3d4e5f7"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None

SCHEMA = "thai"


def upgrade() -> None:
    bind = op.get_bind()
    insp = sa.inspect(bind)
    tables = set(insp.get_table_names(schema=SCHEMA))

    if "Cluster" not in tables:
        op.create_table(
            "Cluster",
            sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
            sa.Column("type", sa.String(length=64), nullable=False),
            sa.Column("subtype", sa.String(length=64), nullable=True),
            sa.Column("value", sa.String(length=64), nullable=False),
            sa.UniqueConstraint("type", "value", name="uq_thai_cluster_type_value"),
            schema=SCHEMA,
        )

    insp = sa.inspect(bind)
    tables = set(insp.get_table_names(schema=SCHEMA))
    if "ClusterWord" not in tables:
        op.create_table(
            "ClusterWord",
            sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
            sa.Column("cluster_id", sa.Integer(), nullable=False),
            sa.Column("word_id", sa.Integer(), nullable=False),
            sa.ForeignKeyConstraint(
                ["cluster_id"],
                ["Cluster.id"],
                ondelete="CASCADE",
                referent_schema=SCHEMA,
            ),
            sa.ForeignKeyConstraint(
                ["word_id"],
                ["Word.id"],
                referent_schema=SCHEMA,
            ),
            sa.UniqueConstraint("cluster_id", "word_id", name="uq_thai_cluster_word_pair"),
            schema=SCHEMA,
        )


def downgrade() -> None:
    bind = op.get_bind()
    insp = sa.inspect(bind)
    existing = set(insp.get_table_names(schema=SCHEMA))
    if "ClusterWord" in existing:
        op.drop_table("ClusterWord", schema=SCHEMA)
    if "Cluster" in existing:
        op.drop_table("Cluster", schema=SCHEMA)
