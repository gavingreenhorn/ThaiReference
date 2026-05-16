"""Recovery: Sound.value IPA + drop Sound.transcription (DDL + DML, idempotent).

Order (required):
  1. ADD Sound.value (nullable)
  2. BACKFILL value from Transcription."IPA" while FK still exists
  3. SET value NOT NULL
  4. DROP Sound.transcription FK + column
  5. DELETE Transcription rows only used by Sound (after FK gone)

Revision ID: f7a8b9c0d1e2
Revises: e5f6a7b8c9d2
Create Date: 2026-05-15
"""

from __future__ import annotations

import sqlalchemy as sa
from alembic import op

revision = "f7a8b9c0d1e2"
down_revision = "e5f6a7b8c9d2"
branch_labels = None
depends_on = None

SCHEMA = "thai"


def _sound_columns(bind) -> set[str]:
    insp = sa.inspect(bind)
    return {c["name"] for c in insp.get_columns("Sound", schema=SCHEMA)}


def upgrade() -> None:
    bind = op.get_bind()
    cols = _sound_columns(bind)

    if "value" not in cols:
        op.add_column(
            "Sound",
            sa.Column("value", sa.Text(), nullable=True),
            schema=SCHEMA,
        )
        cols = _sound_columns(bind)

    if "transcription" in cols:
        op.execute(
            sa.text(
                f"""
                UPDATE {SCHEMA}."Sound" s
                SET value = CASE s.id
                    WHEN 3 THEN 'p'
                    WHEN 6 THEN 't'
                    WHEN 7 THEN 'k'
                    ELSE s.value
                END
                FROM {SCHEMA}."Transcription" t
                WHERE t.id = s.transcription AND s.id IN (3, 6, 7);

                UPDATE {SCHEMA}."Sound" s
                SET value = NULLIF(BTRIM(t."IPA"), '')
                FROM {SCHEMA}."Transcription" t
                WHERE t.id = s.transcription
                  AND (s.value IS NULL OR BTRIM(s.value) = '');

                UPDATE {SCHEMA}."Sound" SET value = '' WHERE value IS NULL;
                """
            )
        )

        op.alter_column("Sound", "value", nullable=False, schema=SCHEMA)

        op.drop_constraint(
            "Sound_transcription_fkey",
            "Sound",
            schema=SCHEMA,
            type_="foreignkey",
        )
        op.drop_column("Sound", "transcription", schema=SCHEMA)

    op.execute(
        sa.text(
            f"""
            DELETE FROM {SCHEMA}."Transcription" t
            WHERE NOT EXISTS (
                SELECT 1 FROM {SCHEMA}."Word" w WHERE w.transcription = t.id
            )
            AND NOT EXISTS (
                SELECT 1 FROM {SCHEMA}."Consonant" c WHERE c.transcription = t.id
            )
            """
        )
    )


def downgrade() -> None:
    bind = op.get_bind()
    cols = _sound_columns(bind)

    if "transcription" not in cols:
        op.add_column(
            "Sound",
            sa.Column("transcription", sa.Integer(), nullable=True),
            schema=SCHEMA,
        )
        op.create_foreign_key(
            "Sound_transcription_fkey",
            "Sound",
            "Transcription",
            ["transcription"],
            ["id"],
            source_schema=SCHEMA,
            referent_schema=SCHEMA,
        )

    if "value" in cols:
        op.alter_column("Sound", "value", nullable=True, schema=SCHEMA)
        op.drop_column("Sound", "value", schema=SCHEMA)
