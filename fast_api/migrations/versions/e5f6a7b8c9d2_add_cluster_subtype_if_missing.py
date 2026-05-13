"""Add thai.Cluster.subtype when table exists without it (legacy / partial DDL).

Revision ID: e5f6a7b8c9d2
Revises: d4e5f6a7b8c0
Create Date: 2026-05-14
"""

from __future__ import annotations

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "e5f6a7b8c9d2"
down_revision: Union[str, Sequence[str], None] = "d4e5f6a7b8c0"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None

SCHEMA = "thai"


def upgrade() -> None:
    bind = op.get_bind()
    insp = sa.inspect(bind)
    if "Cluster" not in insp.get_table_names(schema=SCHEMA):
        return
    cols = {c["name"] for c in insp.get_columns("Cluster", schema=SCHEMA)}
    if "subtype" in cols:
        return
    op.add_column(
        "Cluster",
        sa.Column("subtype", sa.String(length=64), nullable=True),
        schema=SCHEMA,
    )


def downgrade() -> None:
    bind = op.get_bind()
    insp = sa.inspect(bind)
    if "Cluster" not in insp.get_table_names(schema=SCHEMA):
        return
    cols = {c["name"] for c in insp.get_columns("Cluster", schema=SCHEMA)}
    if "subtype" not in cols:
        return
    op.drop_column("Cluster", "subtype", schema=SCHEMA)
