"""Add nullable Lexeme.part_of_speech and Lexeme.tags columns (DDL only).

part_of_speech — actual POS only (noun, verb, adjective, …).
tags           — literal bracket text when it is not a POS (Pali, Tone, mark, …).

Revision ID: b1c2d3e4f5a6
Revises: a8b9c0d1e2f3
Create Date: 2026-05-20
"""

from __future__ import annotations

import sqlalchemy as sa
from alembic import op

revision = "b1c2d3e4f5a6"
down_revision = "a8b9c0d1e2f3"
branch_labels = None
depends_on = None

SCHEMA = "thai"


def _lexeme_columns(bind) -> set[str]:
    insp = sa.inspect(bind)
    return {c["name"] for c in insp.get_columns("Lexeme", schema=SCHEMA)}


def upgrade() -> None:
    bind = op.get_bind()
    cols = _lexeme_columns(bind)
    if "part_of_speech" not in cols:
        op.add_column(
            "Lexeme",
            sa.Column("part_of_speech", sa.String(length=50), nullable=True),
            schema=SCHEMA,
        )

    cols = _lexeme_columns(bind)
    if "qualifier" in cols and "tags" not in cols:
        op.alter_column(
            "Lexeme",
            "qualifier",
            new_column_name="tags",
            schema=SCHEMA,
        )
    elif "tags" not in cols:
        op.add_column(
            "Lexeme",
            sa.Column("tags", sa.String(length=64), nullable=True),
            schema=SCHEMA,
        )


def downgrade() -> None:
    bind = op.get_bind()
    cols = _lexeme_columns(bind)
    if "tags" in cols:
        op.drop_column("Lexeme", "tags", schema=SCHEMA)
    cols = _lexeme_columns(bind)
    if "part_of_speech" in cols:
        op.drop_column("Lexeme", "part_of_speech", schema=SCHEMA)
