# ThaiReference
gh workflow run deploy_backend.yaml --ref main

## Minimal-set comparison

The minimal-set feature preserves stored IPA. Comparison uses NFD and ignores explicit mid-tone
macrons, affricate ties, unreleased-stop and nonsyllabic annotations, and syllable
separators. Other vowel symbols and diacritics remain significant.

One computed group contains all words with the same underlying sound pattern,
ignoring both tone and vowel length. Each word appears in at most one computed
group. For example, เขา, ขาว, ข้าว, เข่า, เข้า, ข่าว, and คาว share one group.
Equivalent pronunciations alone do not form a set. The hand-maintained sets
are unchanged.

## Database changes

V1-V5 are unchanged. Later migrations are consolidated by feature:
V6 minimal-set comparison, V7 season glyphs, and V8 zodiac schema.
Existing glyph backfills are retained to support their required columns.
Future migrations are for features/schema changes, not word-data hotfixes.

Word corrections and lexeme maintenance are applied directly, with source
verification and integrity checks. Synonymous English glosses belong to one
lexeme; distinct senses remain separate. They are not replayed by Flyway.
Fresh databases require a separate reference-data import; migrations do not
recreate the removed word-data fixes.
