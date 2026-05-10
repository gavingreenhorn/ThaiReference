"""CelestialBody (weekday planets) and DayOfWeek.celestial_body_id

Revision ID: c0a1b2d3e4f5
Revises: f2a3b4c5d6e7
Create Date: 2026-05-10
"""
from __future__ import annotations

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "c0a1b2d3e4f5"
down_revision: Union[str, Sequence[str], None] = "f2a3b4c5d6e7"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None

SCHEMA = "thai"

# Ordinal matches DayOfWeek / Thai astrology day order (0 = Sunday …).
# Glyphs use VS15 (U+FE0E) text presentation, matching prior client literals.
PLANETS: list[tuple[int, str, str, str, str]] = [
    (0, "\u2609\uFE0E", "#c1121f", "\u0e1e\u0e23\u0e30\u0e2d\u0e32\u0e17\u0e34\u0e15\u0e22\u0e4c", "Sun"),
    (1, "\u263D\uFE0E", "#e9c46a", "\u0e1e\u0e23\u0e30\u0e08\u0e31\u0e19\u0e17\u0e23\u0e4c", "Moon"),
    (2, "\u2642\uFE0E", "#e5989b", "\u0e1e\u0e23\u0e30\u0e2d\u0e31\u0e07\u0e04\u0e32\u0e23", "Mars"),
    (3, "\u263F\uFE0E", "#2a9d8f", "\u0e1e\u0e23\u0e30\u0e1e\u0e38\u0e18", "Mercury"),
    (4, "\u2643\uFE0E", "#e76f51", "\u0e1e\u0e23\u0e30\u0e1e\u0e24\u0e2b\u0e31\u0e2a\u0e1a\u0e14\u0e35", "Jupiter"),
    (5, "\u2640\uFE0E", "#457b9d", "\u0e1e\u0e23\u0e30\u0e28\u0e38\u0e01\u0e23\u0e4c", "Venus"),
    (6, "\u2644\uFE0E", "#7b2cbf", "\u0e1e\u0e23\u0e30\u0e40\u0e2a\u0e32\u0e23\u0e4c", "Saturn"),
]


def _insert_word_with_lexeme(conn: sa.Connection, surface: str) -> int:
    lex_id = conn.execute(
        sa.text(f'INSERT INTO {SCHEMA}."Lexeme" (value) VALUES (:v) RETURNING id'),
        {"v": surface},
    ).scalar_one()
    wid = conn.execute(
        sa.text(
            f'INSERT INTO {SCHEMA}."Word" ("transcription", value) '
            "VALUES (NULL, :v) RETURNING id"
        ),
        {"v": surface},
    ).scalar_one()
    conn.execute(
        sa.text(
            f'INSERT INTO {SCHEMA}."WordLexeme" ("word", lexeme) VALUES (:w, :l)'
        ),
        {"w": wid, "l": lex_id},
    )
    return int(wid)


def upgrade() -> None:
    op.create_table(
        "CelestialBody",
        sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
        sa.Column("ordinal", sa.Integer(), nullable=False),
        sa.Column("glyph", sa.String(length=32), nullable=False),
        sa.Column("color_hex", sa.String(length=16), nullable=False),
        sa.Column("thai_name_word_id", sa.Integer(), nullable=False),
        sa.Column("english_name_word_id", sa.Integer(), nullable=False),
        sa.UniqueConstraint("ordinal", name="uq_thai_celestial_body_ordinal"),
        schema=SCHEMA,
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
    op.create_foreign_key(
        "fk_celestial_body_english_word",
        "CelestialBody",
        "Word",
        ["english_name_word_id"],
        ["id"],
        source_schema=SCHEMA,
        referent_schema=SCHEMA,
    )
    op.add_column(
        "DayOfWeek",
        sa.Column("celestial_body_id", sa.Integer(), nullable=True),
        schema=SCHEMA,
    )

    bind = op.get_bind()

    for ordinal, glyph, color_hex, thai_surface, eng_surface in PLANETS:
        th_wid = _insert_word_with_lexeme(bind, thai_surface)
        en_wid = _insert_word_with_lexeme(bind, eng_surface)
        bind.execute(
            sa.text(
                f"""
                INSERT INTO {SCHEMA}."CelestialBody"
                    (ordinal, glyph, color_hex, thai_name_word_id, english_name_word_id)
                VALUES (:ord, :glyph, :hex, :th, :en)
                """
            ),
            {
                "ord": ordinal,
                "glyph": glyph,
                "hex": color_hex,
                "th": th_wid,
                "en": en_wid,
            },
        )

    bind.execute(
        sa.text(
            f"""
            UPDATE {SCHEMA}."DayOfWeek" AS d
            SET celestial_body_id = c.id
            FROM {SCHEMA}."CelestialBody" AS c
            WHERE c.ordinal = d.ordinal
            """
        )
    )

    op.create_foreign_key(
        "fk_thai_dayofweek_celestial_body",
        "DayOfWeek",
        "CelestialBody",
        ["celestial_body_id"],
        ["id"],
        source_schema=SCHEMA,
        referent_schema=SCHEMA,
    )
    op.alter_column(
        "DayOfWeek",
        "celestial_body_id",
        nullable=False,
        schema=SCHEMA,
    )


def downgrade() -> None:
    op.drop_constraint(
        "fk_thai_dayofweek_celestial_body",
        "DayOfWeek",
        schema=SCHEMA,
        type_="foreignkey",
    )
    op.drop_column("DayOfWeek", "celestial_body_id", schema=SCHEMA)
    op.drop_constraint(
        "fk_celestial_body_english_word",
        "CelestialBody",
        schema=SCHEMA,
        type_="foreignkey",
    )
    op.drop_constraint(
        "fk_celestial_body_thai_word",
        "CelestialBody",
        schema=SCHEMA,
        type_="foreignkey",
    )
    op.drop_table("CelestialBody", schema=SCHEMA)
