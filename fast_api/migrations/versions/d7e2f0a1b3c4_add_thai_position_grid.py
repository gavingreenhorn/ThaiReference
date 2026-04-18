"""add thai Position / PositionWord grid tables

Revision ID: d7e2f0a1b3c4
Revises: c91d4b2a0e1f
Create Date: 2026-04-18

"""
from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "d7e2f0a1b3c4"
down_revision: Union[str, Sequence[str], None] = "c91d4b2a0e1f"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None

SCHEMA = "thai"


def upgrade() -> None:
    bind = op.get_bind()
    insp = sa.inspect(bind)
    if "Position" not in insp.get_table_names(schema=SCHEMA):
        op.create_table(
            "Position",
            sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
            sa.Column("row_index", sa.Integer(), nullable=False),
            sa.Column("col_index", sa.Integer(), nullable=False),
            sa.Column("name", sa.Text(), nullable=True),
            sa.Column("tile_style", sa.String(length=32), nullable=True),
            sa.UniqueConstraint("row_index", "col_index", name="uq_thai_position_row_col"),
            schema=SCHEMA,
        )
    insp = sa.inspect(bind)
    if "PositionWord" not in insp.get_table_names(schema=SCHEMA):
        op.create_table(
            "PositionWord",
            sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
            sa.Column("position_id", sa.Integer(), nullable=False),
            sa.Column("word_id", sa.Integer(), nullable=False),
            sa.Column("word_order", sa.Integer(), nullable=False),
            sa.ForeignKeyConstraint(
                ["position_id"],
                ["Position.id"],
                ondelete="CASCADE",
                referent_schema=SCHEMA,
            ),
            sa.ForeignKeyConstraint(
                ["word_id"],
                ["Word.id"],
                referent_schema=SCHEMA,
            ),
            sa.UniqueConstraint("position_id", "word_order", name="uq_thai_position_word_order"),
            schema=SCHEMA,
        )


def downgrade() -> None:
    bind = op.get_bind()
    insp = sa.inspect(bind)
    existing = set(insp.get_table_names(schema=SCHEMA))
    if "PositionWord" in existing:
        op.drop_table("PositionWord", schema=SCHEMA)
    if "Position" in existing:
        op.drop_table("Position", schema=SCHEMA)
