"""Add unique constraint on Lexeme (value, part_of_speech).

Revision ID: c3d4e5f6a7b8
Revises: b1c2d3e4f5a6
"""

from alembic import op

revision = "c3d4e5f6a7b8"
down_revision = "b1c2d3e4f5a6"
branch_labels = None
depends_on = None

SCHEMA = "thai"
CONSTRAINT = "uq_thai_lexeme_value_part_of_speech"


def upgrade() -> None:
    op.execute(
        f"""
        ALTER TABLE {SCHEMA}."Lexeme"
        ADD CONSTRAINT {CONSTRAINT}
        UNIQUE NULLS NOT DISTINCT (value, part_of_speech)
        """
    )


def downgrade() -> None:
    op.drop_constraint(CONSTRAINT, "Lexeme", schema=SCHEMA, type_="unique")
