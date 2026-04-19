"""
Operator script: ensure thai.TimePeriod exists, then insert Thai weekday + month
Words (with Lexeme + Transcription) and TimePeriod rows when missing.

Set env: THAIDB_HOST THAIDB_PORT THAIDB_NAME THAIDB_USER THAIDB_PASSWORD
"""
from __future__ import annotations

import os
import sys

import psycopg2

# ISO 8601: Monday = 1 … Sunday = 7
DAYS: list[tuple[int, str, str, str, str, str]] = [
    (1, "จันทร์", "jan", "jan", "tɕan", "Monday"),
    (2, "อังคาร", "ang-khaan", "ang-khaan", "ʔaŋ.kʰaːn", "Tuesday"),
    (3, "พุธ", "phut", "phút", "pʰút", "Wednesday"),
    (4, "พฤหัสบดี", "phrue-hat", "phrɯ́-hàt", "pʰrɯ́.hàt", "Thursday"),
    (5, "ศุกร์", "suk", "sùk", "sùk", "Friday"),
    (6, "เสาร์", "sao", "sǎo", "sǎːw", "Saturday"),
    (7, "อาทิตย์", "aat-thit", "aat-thít", "ʔaːt.tʰít", "Sunday"),
]

MONTHS: list[tuple[int, str, str, str, str, str]] = [
    (1, "มกราคม", "mok-garaa-kom", "mók-kà-raa-kom", "mók.kà.raː.kom", "January"),
    (2, "กุมภาพันธ์", "gum-pha-phan", "gum-pʰaa-pan", "kun.pʰaː.pʰan", "February"),
    (3, "มีนาคม", "mee-naa-khom", "mee-naa-khom", "miː.naː.kʰom", "March"),
    (4, "เมษายน", "meu-saa-yon", "mêe-sǎa-yon", "meː.saː.jon", "April"),
    (5, "พฤษภาคม", "phrue-sa-pha-kom", "phrɯ́-sǎ-pʰaa-kom", "pʰrɯ́.sa.pʰaː.kom", "May"),
    (6, "มิถุนายน", "mi-thu-naa-yon", "mí-tʰù-naa-yon", "mí.tʰù.naː.jon", "June"),
    (7, "กรกฎาคม", "gor-ga-daa-khom", "grà-gà-daa-khom", "krà.ka.daː.kʰom", "July"),
    (8, "สิงหาคม", "sing-haa-khom", "sǐng-hǎa-khom", "sǐŋ.hǎː.kʰom", "August"),
    (9, "กันยายน", "gan-yaa-yon", "gan-yaa-yon", "kan.jaː.jon", "September"),
    (10, "ตุลาคม", "dtu-laa-khom", "dtù-laa-khom", "tù.laː.kʰom", "October"),
    (11, "พฤศจิกายน", "phrue-sa-jii-gaa-yon", "phrɯ́-sà-jì-gaa-yon", "pʰrɯ́.sa.tɕì.kaː.jon", "November"),
    (12, "ธันวาคม", "than-waa-khom", "than-waa-khom", "tʰan.waː.kʰom", "December"),
]


def connect():
    return psycopg2.connect(
        host=os.environ.get("THAIDB_HOST", "127.0.0.1"),
        port=int(os.environ.get("THAIDB_PORT", "5432")),
        dbname=os.environ.get("THAIDB_NAME", "thaidb"),
        user=os.environ.get("THAIDB_USER", "postgres"),
        password=os.environ.get("THAIDB_PASSWORD", ""),
    )


def ensure_time_period_table(cur) -> None:
    cur.execute(
        """
        CREATE TABLE IF NOT EXISTS thai."TimePeriod" (
            id serial NOT NULL,
            kind varchar(32) NOT NULL,
            ordinal integer NOT NULL,
            word_id integer NOT NULL,
            CONSTRAINT pk_thai_time_period PRIMARY KEY (id),
            CONSTRAINT fk_thai_time_period_word
                FOREIGN KEY (word_id) REFERENCES thai."Word" (id),
            CONSTRAINT uq_thai_time_period_kind_ord UNIQUE (kind, ordinal)
        )
        """
    )


def ensure_word(cur, thai: str, phonemic: str, paiboon: str, ipa: str, gloss: str) -> int:
    cur.execute('SELECT id FROM thai."Word" WHERE value = %s', (thai,))
    row = cur.fetchone()
    if row:
        return row[0]
    cur.execute(
        '''INSERT INTO thai."Transcription" (phonemic, paiboon, "IPA")
           VALUES (%s, %s, %s) RETURNING id''',
        (phonemic, paiboon, ipa),
    )
    tid = cur.fetchone()[0]
    cur.execute(
        'INSERT INTO thai."Word" (transcription, value, media) VALUES (%s, %s, NULL) RETURNING id',
        (tid, thai),
    )
    wid = cur.fetchone()[0]
    cur.execute('INSERT INTO thai."Lexeme" (value) VALUES (%s) RETURNING id', (gloss,))
    lid = cur.fetchone()[0]
    cur.execute('INSERT INTO thai."WordLexeme" ("word", lexeme) VALUES (%s, %s)', (wid, lid))
    return wid


def ensure_time_period(cur, kind: str, ordinal: int, word_id: int) -> None:
    cur.execute(
        'SELECT 1 FROM thai."TimePeriod" WHERE kind = %s AND ordinal = %s',
        (kind, ordinal),
    )
    if cur.fetchone():
        return
    cur.execute(
        'INSERT INTO thai."TimePeriod" (kind, ordinal, word_id) VALUES (%s, %s, %s)',
        (kind, ordinal, word_id),
    )


def sync_serial(cur, table_qualified: str) -> None:
    cur.execute("SELECT pg_get_serial_sequence(%s, 'id')", (table_qualified,))
    row = cur.fetchone()
    if not row or not row[0]:
        return
    seq = row[0]
    # table_qualified is one of a fixed trusted list from main()
    cur.execute(f'SELECT COALESCE(MAX(id), 1) FROM {table_qualified}')
    mx = cur.fetchone()[0]
    cur.execute("SELECT setval(%s, %s, true)", (seq, mx))


def main() -> int:
    try:
        conn = connect()
    except Exception as e:
        print(f"connect failed: {e}", file=sys.stderr)
        return 1
    cur = conn.cursor()
    try:
        ensure_time_period_table(cur)
        # Rebuild weekday links so ordinal order can change (e.g. ISO Monday=1).
        cur.execute('DELETE FROM thai."TimePeriod" WHERE kind = %s', ("DayOfWeek",))
        for ord_, thai, ph, pb, ipa, gloss in DAYS:
            wid = ensure_word(cur, thai, ph, pb, ipa, gloss)
            ensure_time_period(cur, "DayOfWeek", ord_, wid)
        for ord_, thai, ph, pb, ipa, gloss in MONTHS:
            wid = ensure_word(cur, thai, ph, pb, ipa, gloss)
            ensure_time_period(cur, "Month", ord_, wid)
        conn.commit()
        for tbl in ('thai."Word"', 'thai."Transcription"', 'thai."Lexeme"', 'thai."WordLexeme"', 'thai."TimePeriod"'):
            sync_serial(cur, tbl)
        conn.commit()
        print("seed ok")
        return 0
    except Exception as e:
        conn.rollback()
        print(f"error: {e}", file=sys.stderr)
        return 1
    finally:
        cur.close()
        conn.close()


if __name__ == "__main__":
    raise SystemExit(main())
