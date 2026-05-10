"""CelestialBody: drop english_name_word_id; rename thai_name_word_id -> word_id + FK.

Revision ID: a1b2c3d4e5f7
Revises: c0a1b2d3e4f5
Create Date: 2026-05-11
"""
from __future__ import annotations

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "a1b2c3d4e5f7"
down_revision: Union[str, Sequence[str], None] = "c0a1b2d3e4f5"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None

SCHEMA = "thai"


def upgrade() -> None:
    bind = op.get_bind()
    insp = sa.inspect(bind)
    cols = {c["name"] for c in insp.get_columns("CelestialBody", schema=SCHEMA)}

    if "english_name_word_id" in cols:
        op.drop_constraint(
            "fk_celestial_body_english_word",
            "CelestialBody",
            schema=SCHEMA,
            type_="foreignkey",
        )
        op.drop_column("CelestialBody", "english_name_word_id", schema=SCHEMA)

    if "word_id" in cols:
        return
    if "thai_name_word_id" not in cols:
        return
    op.drop_constraint(
        "fk_celestial_body_thai_word",
        "CelestialBody",
        schema=SCHEMA,
        type_="foreignkey",
    )
    op.execute(
        sa.text(
            f'ALTER TABLE {SCHEMA}."CelestialBody" RENAME COLUMN thai_name_word_id TO word_id'
        )
    )
    op.create_foreign_key(
        "fk_celestial_body_word",
        "CelestialBody",
        "Word",
        ["word_id"],
        ["id"],
        source_schema=SCHEMA,
        referent_schema=SCHEMA,
    )


def downgrade() -> None:
    bind = op.get_bind()
    insp = sa.inspect(bind)
    cols = {c["name"] for c in insp.get_columns("CelestialBody", schema=SCHEMA)}

    if "word_id" in cols and "thai_name_word_id" not in cols:
        op.drop_constraint(
            "fk_celestial_body_word",
            "CelestialBody",
            schema=SCHEMA,
            type_="foreignkey",
        )
        op.execute(
            sa.text(
                f'ALTER TABLE {SCHEMA}."CelestialBody" RENAME COLUMN word_id TO thai_name_word_id'
            )
        )
        op.create_foreign_key(
            "fk_celestial_body_thai_word",
            "CelestialBody",
            "Word",
            ["thai_name_word_id"],
            ["id"],
            source_schema=SCHEMA,
            referent_schema=SCHEMA,
        )

    if "english_name_word_id" not in cols:
        op.add_column(
            "CelestialBody",
            sa.Column("english_name_word_id", sa.Integer(), nullable=True),
            schema=SCHEMA,
        )
        op.create_foreign_key(
            "fk_celestial_body_english_word",
            "CelestialBody",
            "Word",
            ["english_name_word_id"],
            ["id"],
            source_schema=SCHEMA,
            referent_schema=SCHEMA,
        )
