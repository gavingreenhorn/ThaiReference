"""Add thai.Spelling; Word.spelling_id + is_primary; drop Word.value (DDL only).

Revision ID: a8b9c0d1e2f3
Revises: f7a8b9c0d1e2
Create Date: 2026-05-20
"""

from __future__ import annotations

import sqlalchemy as sa
from alembic import op

revision = "a8b9c0d1e2f3"
down_revision = "f7a8b9c0d1e2"
branch_labels = None
depends_on = None

SCHEMA = "thai"


def _word_columns(bind) -> set[str]:
    insp = sa.inspect(bind)
    return {c["name"] for c in insp.get_columns("Word", schema=SCHEMA)}


def _table_names(bind) -> set[str]:
    return set(sa.inspect(bind).get_table_names(schema=SCHEMA))


def _word_constraints(bind) -> set[str]:
    insp = sa.inspect(bind)
    return {
        c["name"]
        for c in insp.get_unique_constraints("Word", schema=SCHEMA)
    } | {c["name"] for c in insp.get_foreign_keys("Word", schema=SCHEMA)}


def _spelling_id_nullable(bind) -> bool | None:
    insp = sa.inspect(bind)
    for col in insp.get_columns("Word", schema=SCHEMA):
        if col["name"] == "spelling_id":
            return bool(col.get("nullable", True))
    return None


def upgrade() -> None:
    bind = op.get_bind()
    tables = _table_names(bind)

    if "Spelling" not in tables:
        op.create_table(
            "Spelling",
            sa.Column("id", sa.Integer(), primary_key=True, autoincrement=True),
            sa.Column("value", sa.String(length=25), nullable=False),
            sa.UniqueConstraint("value", name="uq_thai_spelling_value"),
            schema=SCHEMA,
        )

    word_cols = _word_columns(bind)

    if "spelling_id" not in word_cols:
        op.add_column(
            "Word",
            sa.Column("spelling_id", sa.Integer(), nullable=True),
            schema=SCHEMA,
        )

    word_cols = _word_columns(bind)
    if "is_primary" not in word_cols:
        op.add_column(
            "Word",
            sa.Column(
                "is_primary",
                sa.Boolean(),
                nullable=False,
                server_default=sa.true(),
            ),
            schema=SCHEMA,
        )

    if _spelling_id_nullable(bind):
        op.alter_column("Word", "spelling_id", nullable=False, schema=SCHEMA)

    constraints = _word_constraints(bind)
    if "Word_spelling_id_fkey" not in constraints:
        op.create_foreign_key(
            "Word_spelling_id_fkey",
            "Word",
            "Spelling",
            ["spelling_id"],
            ["id"],
            source_schema=SCHEMA,
            referent_schema=SCHEMA,
        )

    constraints = _word_constraints(bind)
    if "uq_thai_word_value" in constraints:
        op.drop_constraint("uq_thai_word_value", "Word", schema=SCHEMA, type_="unique")

    constraints = _word_constraints(bind)
    if "uq_thai_word_spelling_transcription" not in constraints:
        op.create_unique_constraint(
            "uq_thai_word_spelling_transcription",
            "Word",
            ["spelling_id", "transcription"],
            schema=SCHEMA,
        )

    word_cols = _word_columns(bind)
    if "value" in word_cols:
        op.drop_column("Word", "value", schema=SCHEMA)


def downgrade() -> None:
    bind = op.get_bind()
    word_cols = _word_columns(bind)
    constraints = _word_constraints(bind)

    if "value" not in word_cols:
        op.add_column(
            "Word",
            sa.Column("value", sa.String(length=25), nullable=True),
            schema=SCHEMA,
        )

    if "uq_thai_word_spelling_transcription" in constraints:
        op.drop_constraint(
            "uq_thai_word_spelling_transcription",
            "Word",
            schema=SCHEMA,
            type_="unique",
        )

    constraints = _word_constraints(bind)
    if "uq_thai_word_value" not in constraints:
        op.create_unique_constraint(
            "uq_thai_word_value",
            "Word",
            ["value"],
            schema=SCHEMA,
        )

    constraints = _word_constraints(bind)
    if "Word_spelling_id_fkey" in constraints:
        op.drop_constraint(
            "Word_spelling_id_fkey", "Word", schema=SCHEMA, type_="foreignkey"
        )

    if _spelling_id_nullable(bind) is False:
        op.alter_column("Word", "spelling_id", nullable=True, schema=SCHEMA)

    word_cols = _word_columns(bind)
    if "spelling_id" in word_cols:
        op.drop_column("Word", "spelling_id", schema=SCHEMA)

    word_cols = _word_columns(bind)
    if "is_primary" in word_cols:
        op.drop_column("Word", "is_primary", schema=SCHEMA)

    tables = _table_names(bind)
    if "Spelling" in tables:
        op.drop_table("Spelling", schema=SCHEMA)
