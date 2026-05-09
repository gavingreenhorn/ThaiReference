"""decouple Month from TimePeriod; add thai Zodiac and Month tables

Revision ID: b3c1e9d4a2f7
Revises: a9d4c8e7f6b1
Create Date: 2026-05-09
"""
from __future__ import annotations

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "b3c1e9d4a2f7"
down_revision: Union[str, Sequence[str], None] = "a9d4c8e7f6b1"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None

SCHEMA = "thai"


# Zodiac sign data ordered Capricorn-first so ordinal 0 maps to January.
# image_url is intentionally NULL; client falls back to a hardcoded unicode glyph
# keyed by ordinal until images are uploaded.
ZODIAC_ROWS = [
    {"ordinal": 0,  "english_name": "Capricorn",   "sanskrit_name": "Makara",   "sanskrit_lexeme": "sea-monster", "element": "Earth"},
    {"ordinal": 1,  "english_name": "Aquarius",    "sanskrit_name": "Kumbha",   "sanskrit_lexeme": "water-pot",   "element": "Air"},
    {"ordinal": 2,  "english_name": "Pisces",      "sanskrit_name": "Mīna",     "sanskrit_lexeme": "fish",        "element": "Water"},
    {"ordinal": 3,  "english_name": "Aries",       "sanskrit_name": "Meṣa",     "sanskrit_lexeme": "ram",         "element": "Fire"},
    {"ordinal": 4,  "english_name": "Taurus",      "sanskrit_name": "Vṛṣabha",  "sanskrit_lexeme": "bull",        "element": "Earth"},
    {"ordinal": 5,  "english_name": "Gemini",      "sanskrit_name": "Mithuna",  "sanskrit_lexeme": "a pair",      "element": "Air"},
    {"ordinal": 6,  "english_name": "Cancer",      "sanskrit_name": "Karkaṭa",  "sanskrit_lexeme": "crab",        "element": "Water"},
    {"ordinal": 7,  "english_name": "Leo",         "sanskrit_name": "Siṃha",    "sanskrit_lexeme": "lion",        "element": "Fire"},
    {"ordinal": 8,  "english_name": "Virgo",       "sanskrit_name": "Kanyā",    "sanskrit_lexeme": "girl",        "element": "Earth"},
    {"ordinal": 9,  "english_name": "Libra",       "sanskrit_name": "Tulā",     "sanskrit_lexeme": "balance",     "element": "Air"},
    {"ordinal": 10, "english_name": "Scorpio",     "sanskrit_name": "Vṛścika",  "sanskrit_lexeme": "scorpion",    "element": "Water"},
    {"ordinal": 11, "english_name": "Sagittarius", "sanskrit_name": "Dhanu",    "sanskrit_lexeme": "bow",         "element": "Fire"},
]

# Month metadata indexed positionally to match the 12 Month TimePeriod rows
# (sorted ASC by ordinal, January-first). Carries short Thai abbreviation and
# English name; the underlying Word record (Thai full name + transcription)
# is reused as-is from the existing TimePeriod row.
MONTH_METADATA_ROWS = [
    {"short_name": "ม.ค.",   "english_name": "January",   "zodiac_ordinal": 0},
    {"short_name": "ก.พ.",   "english_name": "February",  "zodiac_ordinal": 1},
    {"short_name": "มี.ค.",  "english_name": "March",     "zodiac_ordinal": 2},
    {"short_name": "เม.ย.",  "english_name": "April",     "zodiac_ordinal": 3},
    {"short_name": "พ.ค.",   "english_name": "May",       "zodiac_ordinal": 4},
    {"short_name": "มิ.ย.",  "english_name": "June",      "zodiac_ordinal": 5},
    {"short_name": "ก.ค.",   "english_name": "July",      "zodiac_ordinal": 6},
    {"short_name": "ส.ค.",   "english_name": "August",    "zodiac_ordinal": 7},
    {"short_name": "ก.ย.",   "english_name": "September", "zodiac_ordinal": 8},
    {"short_name": "ต.ค.",   "english_name": "October",   "zodiac_ordinal": 9},
    {"short_name": "พ.ย.",   "english_name": "November",  "zodiac_ordinal": 10},
    {"short_name": "ธ.ค.",   "english_name": "December",  "zodiac_ordinal": 11},
]


def _table_names(inspector: sa.Inspector) -> set[str]:
    return set(inspector.get_table_names(schema=SCHEMA))


def _upsert_zodiac(bind: sa.Connection, *, row: dict) -> int:
    existing = bind.execute(
        sa.text(
            f"""
            SELECT id
            FROM {SCHEMA}."Zodiac"
            WHERE ordinal = :ordinal
            LIMIT 1
            """
        ),
        {"ordinal": row["ordinal"]},
    ).scalar_one_or_none()

    if existing is not None:
        bind.execute(
            sa.text(
                f"""
                UPDATE {SCHEMA}."Zodiac"
                SET english_name = :english_name,
                    sanskrit_name = :sanskrit_name,
                    sanskrit_lexeme = :sanskrit_lexeme,
                    element = :element
                WHERE id = :id
                """
            ),
            {**row, "id": int(existing)},
        )
        return int(existing)

    return int(
        bind.execute(
            sa.text(
                f"""
                INSERT INTO {SCHEMA}."Zodiac"
                    (ordinal, english_name, sanskrit_name, sanskrit_lexeme, element, image_url)
                VALUES
                    (:ordinal, :english_name, :sanskrit_name, :sanskrit_lexeme, :element, NULL)
                RETURNING id
                """
            ),
            row,
        ).scalar_one()
    )


def _upsert_month(
    bind: sa.Connection,
    *,
    ordinal: int,
    word_id: int,
    short_name: str,
    english_name: str,
    zodiac_id: int,
) -> None:
    existing = bind.execute(
        sa.text(
            f"""
            SELECT id
            FROM {SCHEMA}."Month"
            WHERE ordinal = :ordinal
            LIMIT 1
            """
        ),
        {"ordinal": ordinal},
    ).scalar_one_or_none()

    params = {
        "ordinal": ordinal,
        "word_id": word_id,
        "short_name": short_name,
        "english_name": english_name,
        "zodiac_id": zodiac_id,
    }

    if existing is not None:
        bind.execute(
            sa.text(
                f"""
                UPDATE {SCHEMA}."Month"
                SET word_id = :word_id,
                    short_name = :short_name,
                    english_name = :english_name,
                    zodiac_id = :zodiac_id
                WHERE id = :id
                """
            ),
            {**params, "id": int(existing)},
        )
        return

    bind.execute(
        sa.text(
            f"""
            INSERT INTO {SCHEMA}."Month"
                (ordinal, word_id, short_name, english_name, zodiac_id)
            VALUES
                (:ordinal, :word_id, :short_name, :english_name, :zodiac_id)
            """
        ),
        params,
    )


def upgrade() -> None:
    bind = op.get_bind()
    inspector = sa.inspect(bind)
    existing_tables = _table_names(inspector)

    # Drop any partial schema from a previous attempt at this revision.
    if "MonthMetadata" in existing_tables:
        op.drop_table("MonthMetadata", schema=SCHEMA)
        existing_tables.discard("MonthMetadata")

    if "Zodiac" not in existing_tables:
        op.create_table(
            "Zodiac",
            sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
            sa.Column("ordinal", sa.Integer(), nullable=False),
            sa.Column("english_name", sa.String(length=32), nullable=False),
            sa.Column("sanskrit_name", sa.String(length=32), nullable=False),
            sa.Column("sanskrit_lexeme", sa.String(length=64), nullable=False),
            sa.Column("element", sa.String(length=16), nullable=False),
            sa.Column("image_url", sa.Text(), nullable=True),
            sa.UniqueConstraint("ordinal", name="uq_thai_zodiac_ordinal"),
            schema=SCHEMA,
        )
        existing_tables.add("Zodiac")

    if "Month" not in existing_tables:
        op.create_table(
            "Month",
            sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
            sa.Column("ordinal", sa.Integer(), nullable=False),
            sa.Column("word_id", sa.Integer(), nullable=False),
            sa.Column("short_name", sa.String(length=16), nullable=False),
            sa.Column("english_name", sa.String(length=32), nullable=False),
            sa.Column("zodiac_id", sa.Integer(), nullable=False),
            sa.ForeignKeyConstraint(
                ["word_id"], ["Word.id"], referent_schema=SCHEMA
            ),
            sa.ForeignKeyConstraint(
                ["zodiac_id"], ["Zodiac.id"], referent_schema=SCHEMA
            ),
            sa.UniqueConstraint("ordinal", name="uq_thai_month_ordinal"),
            schema=SCHEMA,
        )
        existing_tables.add("Month")

    # Phase 1: seed Zodiac rows.
    zodiac_id_by_ordinal: dict[int, int] = {}
    for row in ZODIAC_ROWS:
        zodiac_id_by_ordinal[row["ordinal"]] = _upsert_zodiac(bind, row=row)

    # Phase 2: harvest existing Month rows from TimePeriod (if still there) and
    # upsert into the new standalone Month table, preserving word_id+ordinal.
    month_periods = bind.execute(
        sa.text(
            f"""
            SELECT id, ordinal, word_id
            FROM {SCHEMA}."TimePeriod"
            WHERE kind = 'Month'
            ORDER BY ordinal ASC
            """
        )
    ).mappings().all()

    for slot, period in enumerate(month_periods[: len(MONTH_METADATA_ROWS)]):
        meta = MONTH_METADATA_ROWS[slot]
        zodiac_id = zodiac_id_by_ordinal[int(meta["zodiac_ordinal"])]
        _upsert_month(
            bind,
            ordinal=int(period["ordinal"]),
            word_id=int(period["word_id"]),
            short_name=str(meta["short_name"]),
            english_name=str(meta["english_name"]),
            zodiac_id=zodiac_id,
        )

    # Phase 3: drop kind='Month' rows from TimePeriod – they live in Month now.
    bind.execute(
        sa.text(
            f"""
            DELETE FROM {SCHEMA}."TimePeriod"
            WHERE kind = 'Month'
            """
        )
    )


def downgrade() -> None:
    bind = op.get_bind()
    inspector = sa.inspect(bind)
    existing_tables = _table_names(inspector)

    if "Month" in existing_tables:
        op.drop_table("Month", schema=SCHEMA)
    if "Zodiac" in existing_tables:
        op.drop_table("Zodiac", schema=SCHEMA)
