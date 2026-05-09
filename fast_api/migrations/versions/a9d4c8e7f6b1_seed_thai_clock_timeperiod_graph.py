"""seed thai clock time periods with full word graph

Revision ID: a9d4c8e7f6b1
Revises: 311e4d5be949
Create Date: 2026-05-09
"""
from __future__ import annotations

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "a9d4c8e7f6b1"
down_revision: Union[str, Sequence[str], None] = "311e4d5be949"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None

SCHEMA = "thai"

TIME_OF_DAY_ROWS = [
    {
        "ordinal": 0,
        "thai": "เที่ยงคืน",
        "phonemic": "thiang khuen",
        "paiboon": "thîang-khʉʉn",
        "ipa": "tʰîaŋ kʰɯːn",
        "lexeme": "midnight",
    },
    {
        "ordinal": 1,
        "thai": "ตี",
        "phonemic": "dtii",
        "paiboon": "dtii",
        "ipa": "tiː",
        "lexeme": "pre-dawn",
    },
    {
        "ordinal": 2,
        "thai": "เช้า",
        "phonemic": "chao",
        "paiboon": "cháo",
        "ipa": "tɕʰáːo",
        "lexeme": "morning",
    },
    {
        "ordinal": 3,
        "thai": "เที่ยง",
        "phonemic": "thiang",
        "paiboon": "thîang",
        "ipa": "tʰîaŋ",
        "lexeme": "noon",
    },
    {
        "ordinal": 4,
        "thai": "บ่าย",
        "phonemic": "baai",
        "paiboon": "bàai",
        "ipa": "bàːj",
        "lexeme": "afternoon",
    },
    {
        "ordinal": 5,
        "thai": "เย็น",
        "phonemic": "yen",
        "paiboon": "yen",
        "ipa": "jen",
        "lexeme": "evening",
    },
    {
        "ordinal": 6,
        "thai": "ค่ำ",
        "phonemic": "kham",
        "paiboon": "khâm",
        "ipa": "kʰâm",
        "lexeme": "dusk",
    },
]

# Ordinals must map directly to ThaiHour.hour labels (0..23).
HOUR_ROWS = [
    {"ordinal": 0, "thai": "เที่ยงคืน", "phonemic": "thiang khuen", "paiboon": "thîang-khʉʉn", "ipa": "tʰîaŋ kʰɯːn", "lexeme": "00:00"},
    {"ordinal": 1, "thai": "ตีหนึ่ง", "phonemic": "dtii nueng", "paiboon": "dtii nʉ̀ng", "ipa": "tiː nɯ̀ŋ", "lexeme": "01:00"},
    {"ordinal": 2, "thai": "ตีสอง", "phonemic": "dtii song", "paiboon": "dtii sɔ̌ɔng", "ipa": "tiː sɔ̌ːŋ", "lexeme": "02:00"},
    {"ordinal": 3, "thai": "ตีสาม", "phonemic": "dtii saam", "paiboon": "dtii sǎam", "ipa": "tiː sǎːm", "lexeme": "03:00"},
    {"ordinal": 4, "thai": "ตีสี่", "phonemic": "dtii sii", "paiboon": "dtii sìi", "ipa": "tiː sìː", "lexeme": "04:00"},
    {"ordinal": 5, "thai": "ตีห้า", "phonemic": "dtii haa", "paiboon": "dtii hâa", "ipa": "tiː hâː", "lexeme": "05:00"},
    {"ordinal": 6, "thai": "หกโมงเช้า", "phonemic": "hok moong chao", "paiboon": "hòk moong cháo", "ipa": "hòk moːŋ tɕʰáːo", "lexeme": "06:00"},
    {"ordinal": 7, "thai": "เจ็ดโมง", "phonemic": "cet moong", "paiboon": "cèt moong", "ipa": "tɕèt moːŋ", "lexeme": "07:00"},
    {"ordinal": 8, "thai": "แปดโมง", "phonemic": "paet moong", "paiboon": "pɛ̀ɛt moong", "ipa": "pɛ̀ːt moːŋ", "lexeme": "08:00"},
    {"ordinal": 9, "thai": "เก้าโมง", "phonemic": "kao moong", "paiboon": "kâo moong", "ipa": "kâo moːŋ", "lexeme": "09:00"},
    {"ordinal": 10, "thai": "สิบโมง", "phonemic": "sip moong", "paiboon": "sìp moong", "ipa": "sìp moːŋ", "lexeme": "10:00"},
    {"ordinal": 11, "thai": "สิบเอ็ดโมง", "phonemic": "sip et moong", "paiboon": "sìp-èt moong", "ipa": "sìp ʔèt moːŋ", "lexeme": "11:00"},
    {"ordinal": 12, "thai": "เที่ยง", "phonemic": "thiang", "paiboon": "thîang", "ipa": "tʰîaŋ", "lexeme": "12:00"},
    {"ordinal": 13, "thai": "บ่ายโมง", "phonemic": "baai moong", "paiboon": "bàai moong", "ipa": "bàːj moːŋ", "lexeme": "13:00"},
    {"ordinal": 14, "thai": "บ่ายสอง", "phonemic": "baai song", "paiboon": "bàai sɔ̌ɔng", "ipa": "bàːj sɔ̌ːŋ", "lexeme": "14:00"},
    {"ordinal": 15, "thai": "บ่ายสาม", "phonemic": "baai saam", "paiboon": "bàai sǎam", "ipa": "bàːj sǎːm", "lexeme": "15:00"},
    {"ordinal": 16, "thai": "บ่ายสี่", "phonemic": "baai sii", "paiboon": "bàai sìi", "ipa": "bàːj sìː", "lexeme": "16:00"},
    {"ordinal": 17, "thai": "ห้าโมงเย็น", "phonemic": "haa moong yen", "paiboon": "hâa moong yen", "ipa": "hâː moːŋ jen", "lexeme": "17:00"},
    {"ordinal": 18, "thai": "หกโมงเย็น", "phonemic": "hok moong yen", "paiboon": "hòk moong yen", "ipa": "hòk moːŋ jen", "lexeme": "18:00"},
    {"ordinal": 19, "thai": "หนึ่งทุ่ม", "phonemic": "nueng thum", "paiboon": "nʉ̀ng thûm", "ipa": "nɯ̀ŋ tʰûm", "lexeme": "19:00"},
    {"ordinal": 20, "thai": "สองทุ่ม", "phonemic": "song thum", "paiboon": "sɔ̌ɔng thûm", "ipa": "sɔ̌ːŋ tʰûm", "lexeme": "20:00"},
    {"ordinal": 21, "thai": "สามทุ่ม", "phonemic": "saam thum", "paiboon": "sǎam thûm", "ipa": "sǎːm tʰûm", "lexeme": "21:00"},
    {"ordinal": 22, "thai": "สี่ทุ่ม", "phonemic": "sii thum", "paiboon": "sìi thûm", "ipa": "sìː tʰûm", "lexeme": "22:00"},
    {"ordinal": 23, "thai": "ห้าทุ่ม", "phonemic": "haa thum", "paiboon": "hâa thûm", "ipa": "hâː tʰûm", "lexeme": "23:00"},
]


def _table_names(inspector: sa.Inspector) -> set[str]:
    return set(inspector.get_table_names(schema=SCHEMA))


def _constraint_exists(bind: sa.Connection, table_name: str, constraint_name: str) -> bool:
    return bool(
        bind.execute(
            sa.text(
                """
                SELECT 1
                FROM information_schema.table_constraints
                WHERE table_schema = :schema
                  AND table_name = :table_name
                  AND constraint_name = :constraint_name
                """
            ),
            {"schema": SCHEMA, "table_name": table_name, "constraint_name": constraint_name},
        ).first()
    )


def _get_or_create_transcription(
    bind: sa.Connection,
    *,
    phonemic: str,
    paiboon: str,
    ipa: str,
) -> int:
    existing = bind.execute(
        sa.text(
            f"""
            SELECT id
            FROM {SCHEMA}."Transcription"
            WHERE phonemic = :phonemic
              AND paiboon = :paiboon
              AND "IPA" = :ipa
            LIMIT 1
            """
        ),
        {"phonemic": phonemic, "paiboon": paiboon, "ipa": ipa},
    ).scalar_one_or_none()
    if existing is not None:
        return int(existing)

    return int(
        bind.execute(
            sa.text(
                f"""
                INSERT INTO {SCHEMA}."Transcription" (phonemic, paiboon, "IPA")
                VALUES (:phonemic, :paiboon, :ipa)
                RETURNING id
                """
            ),
            {"phonemic": phonemic, "paiboon": paiboon, "ipa": ipa},
        ).scalar_one()
    )


def _get_or_create_word(bind: sa.Connection, *, value: str, transcription_id: int) -> int:
    row = bind.execute(
        sa.text(
            f"""
            SELECT id, transcription
            FROM {SCHEMA}."Word"
            WHERE value = :value
            ORDER BY id
            LIMIT 1
            """
        ),
        {"value": value},
    ).mappings().first()
    if row is not None:
        word_id = int(row["id"])
        if row["transcription"] is None:
            bind.execute(
                sa.text(
                    f"""
                    UPDATE {SCHEMA}."Word"
                    SET transcription = :transcription_id
                    WHERE id = :word_id
                    """
                ),
                {"transcription_id": transcription_id, "word_id": word_id},
            )
        return word_id

    return int(
        bind.execute(
            sa.text(
                f"""
                INSERT INTO {SCHEMA}."Word" (transcription, value)
                VALUES (:transcription_id, :value)
                RETURNING id
                """
            ),
            {"transcription_id": transcription_id, "value": value},
        ).scalar_one()
    )


def _get_or_create_lexeme(bind: sa.Connection, *, value: str) -> int:
    existing = bind.execute(
        sa.text(
            f"""
            SELECT id
            FROM {SCHEMA}."Lexeme"
            WHERE value = :value
            ORDER BY id
            LIMIT 1
            """
        ),
        {"value": value},
    ).scalar_one_or_none()
    if existing is not None:
        return int(existing)

    return int(
        bind.execute(
            sa.text(
                f"""
                INSERT INTO {SCHEMA}."Lexeme" (value)
                VALUES (:value)
                RETURNING id
                """
            ),
            {"value": value},
        ).scalar_one()
    )


def _ensure_word_lexeme(bind: sa.Connection, *, word_id: int, lexeme_id: int) -> None:
    exists = bind.execute(
        sa.text(
            f"""
            SELECT 1
            FROM {SCHEMA}."WordLexeme"
            WHERE "word" = :word_id
              AND lexeme = :lexeme_id
            LIMIT 1
            """
        ),
        {"word_id": word_id, "lexeme_id": lexeme_id},
    ).first()
    if exists is None:
        bind.execute(
            sa.text(
                f"""
                INSERT INTO {SCHEMA}."WordLexeme" ("word", lexeme)
                VALUES (:word_id, :lexeme_id)
                """
            ),
            {"word_id": word_id, "lexeme_id": lexeme_id},
        )


def _upsert_time_period(bind: sa.Connection, *, kind: str, ordinal: int, word_id: int) -> None:
    exists = bind.execute(
        sa.text(
            f"""
            SELECT id
            FROM {SCHEMA}."TimePeriod"
            WHERE kind = :kind
              AND ordinal = :ordinal
            LIMIT 1
            """
        ),
        {"kind": kind, "ordinal": ordinal},
    ).scalar_one_or_none()
    if exists is None:
        bind.execute(
            sa.text(
                f"""
                INSERT INTO {SCHEMA}."TimePeriod" (kind, ordinal, word_id)
                VALUES (:kind, :ordinal, :word_id)
                """
            ),
            {"kind": kind, "ordinal": ordinal, "word_id": word_id},
        )
    else:
        bind.execute(
            sa.text(
                f"""
                UPDATE {SCHEMA}."TimePeriod"
                SET word_id = :word_id
                WHERE id = :id
                """
            ),
            {"word_id": word_id, "id": int(exists)},
        )


def _insert_kind_rows(bind: sa.Connection, *, kind: str, rows: list[dict[str, str | int]]) -> None:
    for row in rows:
        transcription_id = _get_or_create_transcription(
            bind,
            phonemic=str(row["phonemic"]),
            paiboon=str(row["paiboon"]),
            ipa=str(row["ipa"]),
        )
        word_id = _get_or_create_word(
            bind,
            value=str(row["thai"]),
            transcription_id=transcription_id,
        )
        lexeme_id = _get_or_create_lexeme(bind, value=str(row["lexeme"]))
        _ensure_word_lexeme(bind, word_id=word_id, lexeme_id=lexeme_id)
        _upsert_time_period(
            bind,
            kind=kind,
            ordinal=int(row["ordinal"]),
            word_id=word_id,
        )


def upgrade() -> None:
    bind = op.get_bind()
    inspector = sa.inspect(bind)
    if "TimePeriod" not in _table_names(inspector):
        return

    if not _constraint_exists(bind, table_name="TimePeriod", constraint_name="uq_thai_time_period_kind_ord"):
        op.create_unique_constraint(
            "uq_thai_time_period_kind_ord",
            "TimePeriod",
            ["kind", "ordinal"],
            schema=SCHEMA,
        )

    # Phase 1: TimeOfDay rows.
    _insert_kind_rows(bind, kind="TimeOfDay", rows=TIME_OF_DAY_ROWS)
    # Phase 2: Hour rows (ordinals 0..23 map directly to ThaiHour.hour labels).
    _insert_kind_rows(bind, kind="Hour", rows=HOUR_ROWS)


def downgrade() -> None:
    bind = op.get_bind()
    inspector = sa.inspect(bind)
    if "TimePeriod" not in _table_names(inspector):
        return

    bind.execute(
        sa.text(
            f"""
            DELETE FROM {SCHEMA}."TimePeriod"
            WHERE kind IN ('TimeOfDay', 'Hour')
            """
        )
    )
