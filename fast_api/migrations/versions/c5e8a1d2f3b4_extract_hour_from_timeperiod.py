"""extract Hour into a standalone table; drop kind='Hour' from TimePeriod

Revision ID: c5e8a1d2f3b4
Revises: b3c1e9d4a2f7
Create Date: 2026-05-09
"""
from __future__ import annotations

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "c5e8a1d2f3b4"
down_revision: Union[str, Sequence[str], None] = "b3c1e9d4a2f7"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None

SCHEMA = "thai"


def _table_names(inspector: sa.Inspector) -> set[str]:
    return set(inspector.get_table_names(schema=SCHEMA))


def upgrade() -> None:
    bind = op.get_bind()
    inspector = sa.inspect(bind)
    existing_tables = _table_names(inspector)

    if "Hour" not in existing_tables:
        op.create_table(
            "Hour",
            sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
            sa.Column("ordinal", sa.Integer(), nullable=False),
            sa.Column("word_id", sa.Integer(), nullable=False),
            sa.ForeignKeyConstraint(
                ["word_id"], ["Word.id"], referent_schema=SCHEMA
            ),
            sa.UniqueConstraint("ordinal", name="uq_thai_hour_ordinal"),
            schema=SCHEMA,
        )

    # Move existing kind='Hour' rows from TimePeriod into the new Hour table.
    # Idempotent: ON CONFLICT (ordinal) DO NOTHING — re-running won't duplicate.
    bind.execute(
        sa.text(
            f"""
            INSERT INTO {SCHEMA}."Hour" (ordinal, word_id)
            SELECT ordinal, word_id
            FROM {SCHEMA}."TimePeriod"
            WHERE kind = 'Hour'
            ON CONFLICT (ordinal) DO NOTHING
            """
        )
    )

    bind.execute(
        sa.text(
            f"""
            DELETE FROM {SCHEMA}."TimePeriod"
            WHERE kind = 'Hour'
            """
        )
    )


def downgrade() -> None:
    bind = op.get_bind()
    inspector = sa.inspect(bind)
    existing_tables = _table_names(inspector)

    if "Hour" not in existing_tables:
        return

    # Best-effort: copy hours back into TimePeriod. Skip if (kind,ordinal) already exists.
    bind.execute(
        sa.text(
            f"""
            INSERT INTO {SCHEMA}."TimePeriod" (kind, ordinal, word_id)
            SELECT 'Hour', h.ordinal, h.word_id
            FROM {SCHEMA}."Hour" h
            ON CONFLICT (kind, ordinal) DO NOTHING
            """
        )
    )

    op.drop_table("Hour", schema=SCHEMA)
