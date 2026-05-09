"""extract DayOfWeek and TimeOfDay into standalone tables; drop TimePeriod

Revision ID: e7b2c9d1a4f0
Revises: c5e8a1d2f3b4
Create Date: 2026-05-10
"""
from __future__ import annotations

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "e7b2c9d1a4f0"
down_revision: Union[str, Sequence[str], None] = "c5e8a1d2f3b4"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None

SCHEMA = "thai"


def _table_names(inspector: sa.Inspector) -> set[str]:
    return set(inspector.get_table_names(schema=SCHEMA))


def upgrade() -> None:
    bind = op.get_bind()
    inspector = sa.inspect(bind)
    tables = _table_names(inspector)

    if "DayOfWeek" not in tables:
        op.create_table(
            "DayOfWeek",
            sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
            sa.Column("ordinal", sa.Integer(), nullable=False),
            sa.Column("word_id", sa.Integer(), nullable=False),
            sa.ForeignKeyConstraint(
                ["word_id"], ["Word.id"], referent_schema=SCHEMA
            ),
            sa.UniqueConstraint("ordinal", name="uq_thai_day_of_week_ordinal"),
            schema=SCHEMA,
        )

    if "TimeOfDay" not in tables:
        op.create_table(
            "TimeOfDay",
            sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
            sa.Column("ordinal", sa.Integer(), nullable=False),
            sa.Column("word_id", sa.Integer(), nullable=False),
            sa.ForeignKeyConstraint(
                ["word_id"], ["Word.id"], referent_schema=SCHEMA
            ),
            sa.UniqueConstraint("ordinal", name="uq_thai_time_of_day_ordinal"),
            schema=SCHEMA,
        )

    tables = _table_names(sa.inspect(bind))
    if "TimePeriod" in tables:
        bind.execute(
            sa.text(
                f"""
                INSERT INTO {SCHEMA}."DayOfWeek" (ordinal, word_id)
                SELECT ordinal, word_id
                FROM {SCHEMA}."TimePeriod"
                WHERE kind = 'DayOfWeek'
                ON CONFLICT (ordinal) DO NOTHING
                """
            )
        )
        bind.execute(
            sa.text(
                f"""
                INSERT INTO {SCHEMA}."TimeOfDay" (ordinal, word_id)
                SELECT ordinal, word_id
                FROM {SCHEMA}."TimePeriod"
                WHERE kind = 'TimeOfDay'
                ON CONFLICT (ordinal) DO NOTHING
                """
            )
        )
        bind.execute(
            sa.text(
                f"""
                DELETE FROM {SCHEMA}."TimePeriod"
                WHERE kind IN ('DayOfWeek', 'TimeOfDay')
                """
            )
        )
        bind.execute(sa.text(f'DROP TABLE IF EXISTS {SCHEMA}."TimePeriod" CASCADE'))


def downgrade() -> None:
    bind = op.get_bind()
    inspector = sa.inspect(bind)
    tables = _table_names(inspector)

    if "TimePeriod" not in tables:
        op.create_table(
            "TimePeriod",
            sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
            sa.Column("kind", sa.String(length=32), nullable=False),
            sa.Column("ordinal", sa.Integer(), nullable=False),
            sa.Column("word_id", sa.Integer(), nullable=False),
            sa.ForeignKeyConstraint(
                ["word_id"],
                ["Word.id"],
                referent_schema=SCHEMA,
            ),
            sa.UniqueConstraint("kind", "ordinal", name="uq_thai_time_period_kind_ord"),
            schema=SCHEMA,
        )

    bind.execute(
        sa.text(
            f"""
            INSERT INTO {SCHEMA}."TimePeriod" (kind, ordinal, word_id)
            SELECT 'DayOfWeek', ordinal, word_id
            FROM {SCHEMA}."DayOfWeek"
            ON CONFLICT (kind, ordinal) DO NOTHING
            """
        )
    )
    bind.execute(
        sa.text(
            f"""
            INSERT INTO {SCHEMA}."TimePeriod" (kind, ordinal, word_id)
            SELECT 'TimeOfDay', ordinal, word_id
            FROM {SCHEMA}."TimeOfDay"
            ON CONFLICT (kind, ordinal) DO NOTHING
            """
        )
    )

    after = _table_names(sa.inspect(bind))
    if "DayOfWeek" in after:
        op.drop_table("DayOfWeek", schema=SCHEMA)
    if "TimeOfDay" in after:
        op.drop_table("TimeOfDay", schema=SCHEMA)
