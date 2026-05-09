"""init

Revision ID: f8a3e1b2d4c5
Revises:
Create Date: 2026-03-06 00:00:00.000000

"""
from typing import Sequence, Union

from alembic import op

from app.models import Base

revision: str = "f8a3e1b2d4c5"
down_revision: Union[str, Sequence[str], None] = None
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    Base.metadata.create_all(op.get_bind())


def downgrade() -> None:
    Base.metadata.drop_all(op.get_bind())
