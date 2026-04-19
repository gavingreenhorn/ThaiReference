"""add thai TimePeriod (calendar day / month words)

Revision ID: f1e2d3c4b5a6
Revises: d7e2f0a1b3c4
Create Date: 2026-04-18

"""
from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "f1e2d3c4b5a6"
down_revision: Union[str, Sequence[str], None] = "d7e2f0a1b3c4"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None

SCHEMA = "thai"


def upgrade() -> None:
    bind = op.get_bind()
    insp = sa.inspect(bind)
    if "TimePeriod" not in insp.get_table_names(schema=SCHEMA):
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


def downgrade() -> None:
    bind = op.get_bind()
    insp = sa.inspect(bind)
    if "TimePeriod" in insp.get_table_names(schema=SCHEMA):
        op.drop_table("TimePeriod", schema=SCHEMA)
