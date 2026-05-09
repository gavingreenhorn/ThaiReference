"""add deity and image_url to thai.DayOfWeek

Revision ID: f2a3b4c5d6e7
Revises: e7b2c9d1a4f0
Create Date: 2026-05-10
"""
from __future__ import annotations

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "f2a3b4c5d6e7"
down_revision: Union[str, Sequence[str], None] = "e7b2c9d1a4f0"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None

SCHEMA = "thai"


def upgrade() -> None:
    op.add_column(
        "DayOfWeek",
        sa.Column("deity", sa.Text(), nullable=True),
        schema=SCHEMA,
    )
    op.add_column(
        "DayOfWeek",
        sa.Column("image_url", sa.String(length=2048), nullable=True),
        schema=SCHEMA,
    )


def downgrade() -> None:
    op.drop_column("DayOfWeek", "image_url", schema=SCHEMA)
    op.drop_column("DayOfWeek", "deity", schema=SCHEMA)
