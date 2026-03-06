"""One-time migration: copy all data from a SQLite database into PostgreSQL.

Usage (inside the fast_api container, after `alembic upgrade head`):
    python scripts/seed_from_sqlite.py /path/to/thai_sqlite.db
"""

import sqlite3
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from sqlalchemy import create_engine, text  # noqa: E402

from app.core.config import settings  # noqa: E402
from app.models.orm_tables import Base  # noqa: E402

TABLE_ORDER = [
    "Transcription",
    "SoundType",
    "Lexeme",
    "Class",
    "Ending",
    "Gender",
    "Formality",
    "Plurality",
    "PronounType",
    "Word",
    "Sound",
    "WordLexeme",
    "Consonant",
    "Vowel",
    "Numeral",
    "Pronoun",
    "SpecialSymbol",
    "CompositeName",
    "ConsonantSample",
    "VowelGrapheme",
    "VowelSample",
    "PronounTypeBreakout",
    "ToneMark",
]


def _read_sqlite_table(conn: sqlite3.Connection, table: str) -> list[dict]:
    cur = conn.execute(f'SELECT * FROM "{table}"')
    cols = [desc[0] for desc in cur.description]
    return [dict(zip(cols, row)) for row in cur.fetchall()]


def seed(sqlite_path: str) -> None:
    sq_conn = sqlite3.connect(sqlite_path)
    pg_engine = create_engine(settings.database_url)

    sq_tables = {
        r[0]
        for r in sq_conn.execute(
            "SELECT name FROM sqlite_master WHERE type='table'"
        ).fetchall()
    }

    with pg_engine.begin() as dst:
        dst.execute(text("SET session_replication_role = 'replica'"))

        for table_name in TABLE_ORDER:
            dst_table = Base.metadata.tables.get(table_name)
            if table_name not in sq_tables or dst_table is None:
                print(f"  SKIP  {table_name}")
                continue

            rows = _read_sqlite_table(sq_conn, table_name)
            if not rows:
                print(f"  {table_name}: 0 rows")
                continue

            dst_cols = {c.name for c in dst_table.columns}
            data = [
                {k: v for k, v in row.items() if k in dst_cols}
                for row in rows
            ]
            dst.execute(dst_table.insert(), data)
            print(f"  {table_name}: {len(data)} rows")

        for table_name in TABLE_ORDER:
            if table_name not in Base.metadata.tables:
                continue
            dst.execute(text(
                f"""SELECT setval(pg_get_serial_sequence('"{table_name}"', 'id'), """
                f"""COALESCE((SELECT MAX(id) FROM "{table_name}"), 0) + 1, false)"""
            ))

        dst.execute(text("SET session_replication_role = 'origin'"))

    sq_conn.close()
    print("Seeding complete.")


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python scripts/seed_from_sqlite.py <sqlite_path>")
        sys.exit(1)
    seed(sys.argv[1])
