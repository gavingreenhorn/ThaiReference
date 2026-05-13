"""Apply static Thai cluster seed SQL (no runtime parsing).

Revision ID: d4e5f6a7b8c0
Revises: c2d3e4f5a6b7
Create Date: 2026-05-13

SQL file: ``migrations/sql/d4e5f6a7b8c0_cluster_seed.sql`` (maintain in-repo; optional
regenerator lives next to the matrix markdown under ``~/.cursor/plans/``, see that folder's
``cluster_matrix.py`` — not shipped with this repo).

Downgrade removes only ``Cluster`` / ``ClusterWord`` rows (does not delete shared
Word / Lexeme / Transcription rows).
"""

from __future__ import annotations

from pathlib import Path
from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "d4e5f6a7b8c0"
down_revision: Union[str, Sequence[str], None] = "c2d3e4f5a6b7"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None

# __file__ is migrations/versions/*.py — SQL lives under migrations/sql/
_SEED_SQL = Path(__file__).resolve().parent.parent / "sql" / "d4e5f6a7b8c0_cluster_seed.sql"


def _run_seed_sql() -> None:
    if not _SEED_SQL.is_file():
        raise RuntimeError(f"cluster seed SQL missing: {_SEED_SQL}")
    body = _SEED_SQL.read_text(encoding="utf-8")
    for chunk in body.split("---STATEMENT---"):
        lines = [ln for ln in chunk.splitlines() if ln.strip() and not ln.strip().startswith("--")]
        stmt = "\n".join(lines).strip()
        if not stmt:
            continue
        op.execute(sa.text(stmt))


def upgrade() -> None:
    _run_seed_sql()


def downgrade() -> None:
    op.execute(sa.text('DELETE FROM thai."ClusterWord"'))
    op.execute(sa.text('DELETE FROM thai."Cluster"'))
