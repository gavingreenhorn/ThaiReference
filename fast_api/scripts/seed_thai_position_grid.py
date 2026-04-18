"""
Idempotent seed for thai."Position" / thai."PositionWord" + supporting Word/Lexeme/Transcription.

Connection: PGHOST, PGPORT, PGDATABASE, PGUSER, PGPASSWORD (standard libpq env).
Safe reuse: no hard-coded PKs; re-runnable.

Vocabulary triplets from project plan (blocks A + B). Grid layout is explicit below;
adjust GRID if your UI mockup differs.
"""
from __future__ import annotations

import os
import sys
from typing import Any

import psycopg2
from psycopg2.extensions import connection as PgConnection

SCHEMA = "thai"

# (thai, phonemic, paiboon, ipa, english_gloss) — triplets per plan; gloss for Lexeme.value
VOCAB: list[tuple[str, str | None, str | None, str | None, str]] = [
    ("มุม", "moom", "mum", "mum", "corner"),
    ("กลาง", "glaang", "glaang", "klaːŋ", "middle"),
    ("แถว", "thaaeo", "tɛ̌ɛo", "tʰɛ̌ːw", "row"),
    ("แรก", "raaek", "rɛ̂ɛk", "rɛ̂ːk", "first"),
    ("สุดท้าย", "soot thaai", "sùt táai", "sùt tʰáːj", "last"),
    ("ซ้าย", "saai", "sáai", "sǎːj", "left"),
    ("ขวา", "khwaai", "kwǎa", "kʰwǎː", "right"),
    ("บน", "bon", "bon", "bon", "top"),
    ("ล่าง", "laang", "lâang", "laːŋ", "bottom"),
    ("ตรง", "trong", "trong", "troŋ", "center"),
    ("ตั้ง", "dtang", "dâng", "taŋ", "set"),
    ("ที่", "thii", "thîi", "tʰîː", "there"),
    ("ค่อน", "khawn", "khɔ̂n", "kʰɔ̂n", "toward"),
]

# (row, col, tile_style, english_caption stored in Position.name, ordered Thai words)
GRID: list[tuple[int, int, str, str, list[str]]] = [
    (1, 1, "corner", "Upper-left corner", ["มุม", "ซ้าย", "บน"]),
    (1, 2, "axis", "Top edge, left of center", ["บน", "ซ้าย", "ที่"]),
    (1, 3, "axis", "Top center", ["บน", "ตรง"]),
    (1, 4, "axis", "Top edge, right of center", ["บน", "ขวา", "ที่"]),
    (1, 5, "corner", "Upper-right corner", ["มุม", "ขวา", "บน"]),
    (2, 1, "axis", "Left edge, upper row", ["ซ้าย", "แถว", "แรก"]),
    (2, 2, "interior", "Upper-left (inside grid)", ["ซ้าย", "บน", "ค่อน"]),
    (2, 3, "interior", "Upper middle (inside grid)", ["บน", "ตรง", "ที่"]),
    (2, 4, "interior", "Upper-right (inside grid)", ["ขวา", "บน", "ค่อน"]),
    (2, 5, "axis", "Right edge, upper row", ["ขวา", "แถว", "แรก"]),
    (3, 1, "axis", "Left middle", ["ซ้าย", "กลาง"]),
    (3, 2, "interior", "Middle-left (inside grid)", ["ซ้าย", "ตรง", "ที่"]),
    (3, 3, "center", "Center", ["กลาง"]),
    (3, 4, "interior", "Middle-right (inside grid)", ["ขวา", "ตรง", "ที่"]),
    (3, 5, "axis", "Right middle", ["ขวา", "กลาง"]),
    (4, 1, "axis", "Left edge, lower row", ["ซ้าย", "แถว", "สุดท้าย"]),
    (4, 2, "interior", "Lower-left (inside grid)", ["ซ้าย", "ล่าง", "ค่อน"]),
    (4, 3, "interior", "Lower middle (inside grid)", ["ล่าง", "ตรง", "ที่"]),
    (4, 4, "interior", "Lower-right (inside grid)", ["ขวา", "ล่าง", "ค่อน"]),
    (4, 5, "axis", "Right edge, lower row", ["ขวา", "แถว", "สุดท้าย"]),
    (5, 1, "corner", "Lower-left corner", ["มุม", "ซ้าย", "ล่าง"]),
    (5, 2, "axis", "Bottom edge, left of center", ["ล่าง", "ซ้าย", "ที่"]),
    (5, 3, "axis", "Bottom center", ["ล่าง", "ตรง"]),
    (5, 4, "axis", "Bottom edge, right of center", ["ล่าง", "ขวา", "ที่"]),
    (5, 5, "corner", "Lower-right corner", ["มุม", "ขวา", "ล่าง"]),
]


def _connect() -> PgConnection:
    return psycopg2.connect(
        host=os.environ.get("PGHOST", "localhost"),
        port=os.environ.get("PGPORT", "5432"),
        dbname=os.environ.get("PGDATABASE", "thai"),
        user=os.environ.get("PGUSER", "admin"),
        password=os.environ.get("PGPASSWORD", ""),
    )


def ensure_word(cur: Any, thai: str, phonemic: str | None, paiboon: str | None, ipa: str | None) -> int:
    cur.execute(f'SELECT id FROM {SCHEMA}."Word" WHERE value = %s LIMIT 1', (thai,))
    row = cur.fetchone()
    if row:
        return int(row[0])

    cur.execute(
        f"""
        SELECT id FROM {SCHEMA}."Transcription" t
        WHERE t.phonemic IS NOT DISTINCT FROM %s
          AND t.paiboon IS NOT DISTINCT FROM %s
          AND t."IPA" IS NOT DISTINCT FROM %s
        LIMIT 1
        """,
        (phonemic, paiboon, ipa),
    )
    tr = cur.fetchone()
    if tr:
        tid = int(tr[0])
    else:
        cur.execute(
            f'INSERT INTO {SCHEMA}."Transcription" (phonemic, paiboon, "IPA") VALUES (%s,%s,%s) RETURNING id',
            (phonemic, paiboon, ipa),
        )
        tid = int(cur.fetchone()[0])

    cur.execute(
        f'INSERT INTO {SCHEMA}."Word" (transcription, value) VALUES (%s,%s) RETURNING id',
        (tid, thai),
    )
    return int(cur.fetchone()[0])


def ensure_lexeme_link(cur: Any, word_id: int, gloss: str) -> None:
    cur.execute(f'SELECT id FROM {SCHEMA}."Lexeme" WHERE value = %s LIMIT 1', (gloss,))
    lr = cur.fetchone()
    if lr:
        lid = int(lr[0])
    else:
        cur.execute(f'INSERT INTO {SCHEMA}."Lexeme" (value) VALUES (%s) RETURNING id', (gloss,))
        lid = int(cur.fetchone()[0])
    cur.execute(
        f"""
        INSERT INTO {SCHEMA}."WordLexeme" ("word", lexeme)
        SELECT %s, %s
        WHERE NOT EXISTS (
            SELECT 1 FROM {SCHEMA}."WordLexeme" wl
            WHERE wl."word" = %s AND wl.lexeme = %s
        )
        """,
        (word_id, lid, word_id, lid),
    )


def ensure_position(cur: Any, row: int, col: int, name: str, tile_style: str) -> int:
    cur.execute(
        f'SELECT id FROM {SCHEMA}."Position" WHERE row_index = %s AND col_index = %s',
        (row, col),
    )
    pr = cur.fetchone()
    if pr:
        pid = int(pr[0])
        cur.execute(
            f'UPDATE {SCHEMA}."Position" SET name = %s, tile_style = %s WHERE id = %s',
            (name, tile_style, pid),
        )
        return pid
    cur.execute(
        f"""
        INSERT INTO {SCHEMA}."Position" (row_index, col_index, name, tile_style)
        VALUES (%s,%s,%s,%s) RETURNING id
        """,
        (row, col, name, tile_style),
    )
    return int(cur.fetchone()[0])


def ensure_position_word(cur: Any, position_id: int, word_id: int, word_order: int) -> None:
    cur.execute(
        f"""
        INSERT INTO {SCHEMA}."PositionWord" (position_id, word_id, word_order)
        SELECT %s, %s, %s
        WHERE NOT EXISTS (
            SELECT 1 FROM {SCHEMA}."PositionWord" x
            WHERE x.position_id = %s AND x.word_order = %s
        )
        """,
        (position_id, word_id, word_order, position_id, word_order),
    )


def main() -> int:
    vocab_map = {t[0]: t for t in VOCAB}
    for _, _, _, _, words in GRID:
        for w in words:
            if w not in vocab_map:
                print(f"Unknown Thai in GRID not in VOCAB: {w!r}", file=sys.stderr)
                return 1

    conn = _connect()
    try:
        conn.autocommit = False
        with conn.cursor() as cur:
            for thai, phonemic, paiboon, ipa, gloss in VOCAB:
                wid = ensure_word(cur, thai, phonemic, paiboon, ipa)
                ensure_lexeme_link(cur, wid, gloss)

            for row, col, tile_style, caption, words in GRID:
                pid = ensure_position(cur, row, col, caption, tile_style)
                for order, thai in enumerate(words, start=1):
                    phonemic, paiboon, ipa, gloss = vocab_map[thai][1], vocab_map[thai][2], vocab_map[thai][3], vocab_map[thai][4]
                    wid = ensure_word(cur, thai, phonemic, paiboon, ipa)
                    ensure_lexeme_link(cur, wid, gloss)
                    ensure_position_word(cur, pid, wid, order)

        conn.commit()
        print("seed_thai_position_grid: committed OK")
        return 0
    except Exception:
        conn.rollback()
        raise
    finally:
        conn.close()


if __name__ == "__main__":
    raise SystemExit(main())
