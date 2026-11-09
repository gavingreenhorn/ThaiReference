# ThaiReference
gh workflow run deploy_backend.yaml --ref main

## Minimal-set comparison

V13 preserves stored IPA. Comparison uses NFD and ignores explicit mid-tone
macrons, affricate ties, unreleased-stop and nonsyllabic annotations, and syllable
separators. Other vowel symbols and diacritics remain significant.

One computed group contains all words with the same underlying sound pattern,
ignoring both tone and vowel length. Each word appears in at most one computed
group. For example, เขา, ขาว, ข้าว, เข่า, เข้า, ข่าว, and คาว share one group.
Equivalent pronunciations alone do not form a set. The hand-maintained sets
are unchanged.

V12 reconciles the word deduplication, missing คาว dependencies, and verified
IPA corrections. Already-corrected databases retain their existing IDs and
links. Flyway applies and records revisions during backend deployment;
do not apply them separately to the live database.
