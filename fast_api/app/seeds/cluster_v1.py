"""Seed ``thai.Cluster`` (``type``, ``value``) and ``thai.ClusterWord``.

Called at the end of migration ``d4e5f6a7b8c0`` when you run
``alembic upgrade head`` (passes the Alembic connection). Idempotent: if
``Cluster`` already has rows, returns without inserting. Romanization is a
first-pass placeholder.
"""

from __future__ import annotations

import sqlalchemy as sa

_RAW_WORDS: list[tuple[str, str, str]] = [
    ("กว่า", "gwàa", "gwàa"),
    ("ใคร่", "krài", "krài"),
    ("กล้อง", "glâawng", "glâawng"),
    ("พรวน", "pruaanM", "pruaanM"),
    ("ตรวจ", "dtrùat", "dtrùat"),
    ("ปลูก", "bplùuk", "bplùuk"),
    ("สร้าง", "sâang", "sâang"),
    ("ทราบ", "sâap", "sâap"),
    ("โทร", "thoohn", "thoohn"),
    ("สมุทร", "sà-mùt", "sà-mùt"),
    ("ไซร้", "sái", "sái"),
    ("จริง", "jing", "jing"),
    ("ศารท", "sǎat", "sǎat"),
    ("สามารถ", "sǎa-mâat", "sǎa-mâat"),
    ("หมา", "mǎa", "mǎa"),
    ("หยุด", "yùt", "yùt"),
    ("หรือ", "rǔue", "rǔue"),
    ("หลับ", "làp", "làp"),
    ("หวัง", "wǎng", "wǎng"),
    ("หง่อย", "ngàawy", "ngàawy"),
    ("อยู่", "yùu", "yùu"),
    ("อย่า", "yàa", "yàa"),
    ("อย่าง", "yàang", "yàang"),
    ("อยาก", "yàak", "yàak"),
    ("สนาม", "sà-nǎam", "sà-nǎam"),
    ("ถนน", "thà-nǒn", "thà-nǒn"),
    ("นคร", "ná-khǒn", "ná-khǒn"),
    ("เฉลียง", "chà-liiang", "chà-liiang"),
    ("เจริญ", "jà-rin", "jà-rin"),
    ("บริการ", "bà-ri-gaan", "bà-ri-gaan"),
    ("มรกต", "má-rák", "má-rák"),
    ("ขนม", "khà-nǒm", "khà-nǒm"),
    ("แขนง", "khà-nǎeng", "khà-nǎeng"),
    ("สนุก", "sà-nùk", "sà-nùk"),
    ("ไฉน", "chǎ-nǎi", "chǎ-nǎi"),
    ("โปรด", "bpròht", "bpròht"),
    ("เปล่า", "bplàao", "bplàao"),
    ("พริ้ม", "prím", "prím"),
    ("เสมอ", "sà-mǒe", "sà-mǒe"),
    ("แสดง", "sà-dǎeng", "sà-dǎeng"),
    ("เสบียง", "sà-bǐiang", "sà-bǐiang"),
    ("เผอิญ", "phà-ǒin", "phà-ǒin"),
    ("เผดียง", "phà-dǐiang", "phà-dǐiang"),
    ("เสด็จ", "sà-dèt", "sà-dèt"),
    ("เผด็จ", "phà-dèt", "phà-dèt"),
    ("เกษียณ", "gà-sǐan", "gà-sǐan"),
    ("เกษียร", "gà-sǐan", "gà-sǐan"),
    ("เฉพาะ", "chà-pháw", "chà-pháw"),
    ("เผชิญ", "phà-chǐn", "phà-chǐn"),
    ("หญิง", "yǐng", "yǐng"),
    ("หณิง", "nǐng", "nǐng"),
    ("หฬาย", "lǎai", "lǎai"),
]


def _word_table() -> dict[str, tuple[str, str]]:
    out: dict[str, tuple[str, str]] = {}
    for s, p, pb in _RAW_WORDS:
        out[s] = (p, pb)
    return out


def _ensure_word(conn: sa.Connection, schema: str, surface: str, phonemic: str, paiboon: str) -> int:
    row = conn.execute(
        sa.text(f'SELECT id FROM {schema}."Word" WHERE value = :v'),
        {"v": surface},
    ).fetchone()
    if row:
        return int(row[0])
    tid = conn.execute(
        sa.text(
            f'INSERT INTO {schema}."Transcription" (phonemic, paiboon, "IPA") '
            "VALUES (:p, :pb, NULL) RETURNING id"
        ),
        {"p": phonemic, "pb": paiboon},
    ).scalar_one()
    wid = conn.execute(
        sa.text(
            f'INSERT INTO {schema}."Word" ("transcription", value) VALUES (:t, :v) RETURNING id'
        ),
        {"t": tid, "v": surface},
    ).scalar_one()
    lid = conn.execute(
        sa.text(f'INSERT INTO {schema}."Lexeme" (value) VALUES (:v) RETURNING id'),
        {"v": surface},
    ).scalar_one()
    conn.execute(
        sa.text(f'INSERT INTO {schema}."WordLexeme" ("word", lexeme) VALUES (:w, :l)'),
        {"w": wid, "l": lid},
    )
    return int(wid)


def _insert_cluster(conn: sa.Connection, schema: str, ctype: str, cvalue: str) -> int:
    return int(
        conn.execute(
            sa.text(
                f'INSERT INTO {schema}."Cluster" ("type", "value") '
                "VALUES (:t, :v) RETURNING id"
            ),
            {"t": ctype, "v": cvalue},
        ).scalar_one()
    )


def _link_word(conn: sa.Connection, schema: str, cluster_id: int, word_id: int, sort_order: int) -> None:
    conn.execute(
        sa.text(
            f'INSERT INTO {schema}."ClusterWord" (cluster_id, word_id, sort_order) '
            "VALUES (:c, :w, :o)"
        ),
        {"c": cluster_id, "w": word_id, "o": sort_order},
    )


def apply_cluster_v1_seed(conn: sa.Connection, schema: str = "thai") -> None:
    n = conn.execute(sa.text(f'SELECT COUNT(*) FROM {schema}."Cluster"')).scalar_one()
    if int(n) > 0:
        return

    wt = _word_table()
    wmap = {s: _ensure_word(conn, schema, s, p, pb) for s, (p, pb) in wt.items()}

    first = ["ก", "ข", "ค", "ต", "ป", "ผ", "พ"]
    second = ["ร", "ล", "ว"]
    un_true = {("ผ", "ร"), ("ป", "ว"), ("ผ", "ว"), ("พ", "ว")}

    key_to_id: dict[tuple[str, str], int] = {}

    for c1 in first:
        for c2 in second:
            if (c1, c2) in un_true:
                continue
            val = c1 + c2
            cid = _insert_cluster(conn, schema, "true", val)
            key_to_id[("true", val)] = cid

    true_links = [
        (("true", "กว"), "กว่า"),
        (("true", "คร"), "ใคร่"),
        (("true", "กล"), "กล้อง"),
        (("true", "พร"), "พรวน"),
        (("true", "ตร"), "ตรวจ"),
        (("true", "ปล"), "ปลูก"),
    ]
    for key, surf in true_links:
        if key in key_to_id and surf in wmap:
            _link_word(conn, schema, key_to_id[key], wmap[surf], 0)

    false_rows: list[tuple[str, str, list[str]]] = [
        ("false_tr_s", "ทร", ["ทราบ"]),
        ("false_tr_t", "ทร", ["โทร"]),
        ("false_tr_fin", "ทร", ["สมุทร"]),
        ("false_sr", "สร", ["สร้าง"]),
        ("false_zr", "ซร", ["ไซร้"]),
        ("false_jr", "จร", ["จริง"]),
        ("false_thr", "ศร", ["ศารท"]),
        ("false_rr", "รร", ["สามารถ"]),
    ]
    for ctype, cval, samples in false_rows:
        cid = _insert_cluster(conn, schema, ctype, cval)
        key_to_id[(ctype, cval)] = cid
        for o, surf in enumerate(samples):
            if surf in wmap:
                _link_word(conn, schema, cid, wmap[surf], o)

    son = ["ง", "น", "ม", "ย", "ร", "ล", "ว", "ญ", "ณ", "ฬ"]
    lead_samples = {
        "ง": "หง่อย",
        "น": "หนู",
        "ม": "หมา",
        "ย": "หยุด",
        "ร": "หรือ",
        "ล": "หลับ",
        "ว": "หวัง",
        "ญ": "หญิง",
        "ณ": "หณิง",
        "ฬ": "หฬาย",
    }
    for g in son:
        val = "ห" + g
        cid = _insert_cluster(conn, schema, "leading_tone", val)
        key_to_id[("leading_tone", val)] = cid
        surf = lead_samples.get(g)
        if surf and surf in wmap:
            _link_word(conn, schema, cid, wmap[surf], 0)

    cid_oy = _insert_cluster(conn, schema, "leading_tone", "อย")
    key_to_id[("leading_tone", "อย")] = cid_oy
    for o, surf in enumerate(["อยู่", "อย่า", "อย่าง", "อยาก"]):
        if surf in wmap:
            _link_word(conn, schema, cid_oy, wmap[surf], o)

    enep: list[tuple[str, list[str]]] = [
        ("สน", ["สนุก"]),
        ("ถน", ["ถนน"]),
        ("นค", ["นคร"]),
        ("ฉล", ["เฉลียง"]),
        ("เจร", ["เจริญ"]),
        ("บร", ["บริการ"]),
        ("มร", ["มรกต"]),
        ("ขน", ["ขนม"]),
        ("แขน", ["แขนง"]),
        ("สด", ["แสดง"]),
        ("เสบ", ["เสบียง"]),
        ("เผอ", ["เผอิญ"]),
        ("เผด", ["เผดียง", "เผด็จ"]),
        ("เสด", ["เสด็จ"]),
        ("เกษ", ["เกษียณ", "เกษียร"]),
        ("เฉพ", ["เฉพาะ"]),
        ("เผช", ["เผชิญ"]),
    ]
    for cval, samples in enep:
        cid = _insert_cluster(conn, schema, "leading_enepenthetic", cval)
        key_to_id[("leading_enepenthetic", cval)] = cid
        for o, surf in enumerate(samples):
            if surf in wmap:
                _link_word(conn, schema, cid, wmap[surf], o)

    cid_fin = _insert_cluster(conn, schema, "final_rt", "รธท")
    key_to_id[("final_rt", "รธท")] = cid_fin
    for o, surf in enumerate(["ศารท", "สามารถ"]):
        if surf in wmap:
            _link_word(conn, schema, cid_fin, wmap[surf], o)
