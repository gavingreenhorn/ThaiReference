"""
migration script: copy all data from a SQLite database into PostgreSQL.
"""
import sqlite3
import sys
from pathlib import Path

from sqlalchemy import create_engine, text

# Add app to path so we can import settings
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))
from app.core.config import settings  # noqa: E402


def _read_sqlite_table(conn: sqlite3.Connection, table: str) -> list[dict]:
    cur = conn.execute(f'SELECT * FROM "{table}"')
    columns = [d[0] for d in cur.description]
    return [dict(zip(columns, row)) for row in cur.fetchall()]


def _pg_columns_for_table(pg_conn, table_name: str) -> set:
    """Return set of column names that exist in Postgres for this table."""
    r = pg_conn.execute(
        text(
            """
            SELECT column_name FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = :t
            """
        ),
        {"t": table_name},
    )
    return {row[0] for row in r}


def _pg_boolean_columns(pg_conn, table_name: str) -> set:
    """Return set of column names that are boolean in Postgres for this table."""
    r = pg_conn.execute(
        text(
            """
            SELECT column_name FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = :t AND data_type = 'boolean'
            """
        ),
        {"t": table_name},
    )
    return {row[0] for row in r}


def _coerce_row_for_pg(row: dict, boolean_columns: set) -> dict:
    """Coerce SQLite 0/1 to Python bool for boolean columns."""
    out = dict(row)
    for col in boolean_columns:
        if col in out and out[col] is not None:
            out[col] = bool(int(out[col]))
    return out


def seed(sqlite_path: str) -> None:
    sq_conn = sqlite3.connect(sqlite_path)
    sq_conn.row_factory = sqlite3.Row
    pg_engine = create_engine(settings.dsn)

    cur = sq_conn.execute(
        "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'"
    )
    table_names = [row[0] for row in cur.fetchall()]

    with pg_engine.connect() as pg_conn:
        # Only seed tables that exist in Postgres (skip e.g. old "Tone" if schema has ToneMark only)
        pg_tables_result = pg_conn.execute(
            text(
                "SELECT tablename FROM pg_tables WHERE schemaname = 'public'"
            )
        )
        pg_table_set = {row[0] for row in pg_tables_result}

        # Disable FK checks so insert order doesn't matter
        pg_conn.execute(text("SET session_replication_role = replica"))
        pg_conn.commit()

        tables_to_seed = [t for t in table_names if t in pg_table_set]
        if tables_to_seed:
            truncate_list = ", ".join(f'"{t}"' for t in tables_to_seed)
            pg_conn.execute(text(f"TRUNCATE TABLE {truncate_list} CASCADE"))
            pg_conn.commit()

        for table_name in table_names:
            if table_name not in pg_table_set:
                print(f"  {table_name}: skip (no such table in Postgres)")
                continue
            rows = _read_sqlite_table(sq_conn, table_name)
            if not rows:
                continue
            pg_cols = _pg_columns_for_table(pg_conn, table_name)
            boolean_cols = _pg_boolean_columns(pg_conn, table_name)
            # Only insert columns that exist in Postgres (SQLite may have extra/dropped cols)
            columns = [c for c in rows[0].keys() if c in pg_cols]
            if not columns:
                print(f"  {table_name}: skip (no matching columns)")
                continue
            placeholders = ", ".join(f":{c}" for c in columns)
            col_list = ", ".join(f'"{c}"' for c in columns)
            stmt = text(
                f'INSERT INTO "{table_name}" ({col_list}) VALUES ({placeholders})'
            )
            for row in rows:
                filtered = {c: row[c] for c in columns}
                pg_conn.execute(stmt, _coerce_row_for_pg(filtered, boolean_cols))
            pg_conn.commit()
            print(f"  {table_name}: {len(rows)} rows")

        pg_conn.execute(text("SET session_replication_role = DEFAULT"))
        pg_conn.commit()

    sq_conn.close()
    print("Done.")


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python scripts/seed_from_sqlite.py <sqlite_path>")
        sys.exit(1)
    seed(sys.argv[1])
