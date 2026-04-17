"""add Vowel mediapath

Revision ID: c91d4b2a0e1f
Revises: f8a3e1b2d4c5
Create Date: 2026-04-17

"""
from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "c91d4b2a0e1f"
down_revision: Union[str, Sequence[str], None] = "f8a3e1b2d4c5"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    bind = op.get_bind()
    insp = sa.inspect(bind)
    cols = {c["name"] for c in insp.get_columns("Vowel")}
    if "mediapath" not in cols:
        op.add_column("Vowel", sa.Column("mediapath", sa.Text(), nullable=True))


def downgrade() -> None:
    bind = op.get_bind()
    insp = sa.inspect(bind)
    cols = {c["name"] for c in insp.get_columns("Vowel")}
    if "mediapath" in cols:
        op.drop_column("Vowel", "mediapath")
