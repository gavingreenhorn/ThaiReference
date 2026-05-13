"""Add thai.Cluster (type, value) + ClusterWord and seed v1.

Runs when you ``alembic upgrade head``: creates tables if missing, then
``apply_cluster_v1_seed`` fills ``Cluster`` / ``ClusterWord`` (skips if
``Cluster`` already has rows).

Revision ID: d4e5f6a7b8c0
Revises: a1b2c3d4e5f7
Create Date: 2026-05-10
"""

from __future__ import annotations

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

from app.seeds.cluster_v1 import apply_cluster_v1_seed

revision: str = "d4e5f6a7b8c0"
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
            sa.Column("sort_order", sa.Integer(), nullable=False, server_default="0"),
            sa.ForeignKeyConstraint(
                ["cluster_id"],
                ["Cluster.id"],
                name="fk_thai_cluster_word_cluster",
                ondelete="CASCADE",
                source_schema=SCHEMA,
                referent_schema=SCHEMA,
            ),
            sa.ForeignKeyConstraint(
                ["word_id"],
                ["Word.id"],
                name="fk_thai_cluster_word_word",
                source_schema=SCHEMA,
                referent_schema=SCHEMA,
            ),
            sa.UniqueConstraint("cluster_id", "word_id", name="uq_thai_cluster_word_pair"),
            schema=SCHEMA,
        )

    apply_cluster_v1_seed(bind, SCHEMA)


def downgrade() -> None:
    bind = op.get_bind()
    insp = sa.inspect(bind)
    existing = set(insp.get_table_names(schema=SCHEMA))
    if "ClusterWord" in existing:
        op.drop_table("ClusterWord", schema=SCHEMA)
    if "Cluster" in existing:
        op.drop_table("Cluster", schema=SCHEMA)
