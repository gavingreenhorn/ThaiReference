"""Add Sound.value (canonical IPA), migrate all sounds, drop Sound.transcription.

Every row in thai."Sound" is migrated — consonant phones (consonant_sound=1) and
vowel/diphthong phones (consonant_sound=0). Vowels reference these via Vowel.sound_id.

For each Sound:
  - Copy Transcription."IPA" into Sound.value (with fixes for sound ids 3, 6, 7).
  - Drop the Sound.transcription FK.

Then delete Transcription rows that were *only* linked from Sound (no Word or
Consonant still references them). Word and Consonant transcriptions are untouched.

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


def upgrade() -> None:
    op.add_column(
        "Sound",
        sa.Column("value", sa.Text(), nullable=True),
        schema=SCHEMA,
    )

    # All sounds: consonant + vowel inventory (entire Sound table).
    op.execute(
        sa.text(
            f"""
            UPDATE {SCHEMA}."Sound" s
            SET value = CASE s.id
                WHEN 3 THEN 'p'
                WHEN 6 THEN 't'
                WHEN 7 THEN 'k'
                ELSE NULL
            END
            FROM {SCHEMA}."Transcription" t
            WHERE t.id = s.transcription AND s.id IN (3, 6, 7);

            UPDATE {SCHEMA}."Sound" s
            SET value = NULLIF(BTRIM(t."IPA"), '')
            FROM {SCHEMA}."Transcription" t
            WHERE t.id = s.transcription AND s.value IS NULL;

            UPDATE {SCHEMA}."Sound" SET value = '' WHERE value IS NULL;
            """
        )
    )

    op.alter_column("Sound", "value", nullable=False, schema=SCHEMA)

    # Orphan only sound-exclusive transcriptions (vowel sounds 25–57, consonant 1–24, etc.).
    op.execute(
        sa.text(
            f"""
            DELETE FROM {SCHEMA}."Transcription" t
            WHERE t.id IN (
                SELECT s.transcription FROM {SCHEMA}."Sound" s
                WHERE s.transcription IS NOT NULL
            )
            AND NOT EXISTS (
                SELECT 1 FROM {SCHEMA}."Word" w WHERE w.transcription = t.id
            )
            AND NOT EXISTS (
                SELECT 1 FROM {SCHEMA}."Consonant" c WHERE c.transcription = t.id
            );
            """
        )
    )

    op.drop_constraint(
        "Sound_transcription_fkey",
        "Sound",
        schema=SCHEMA,
        type_="foreignkey",
    )
    op.drop_column("Sound", "transcription", schema=SCHEMA)


def downgrade() -> None:
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
    op.drop_column("Sound", "value", schema=SCHEMA)
    # Note: deleted Transcription rows are not restored on downgrade.
