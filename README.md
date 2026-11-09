# ThaiReference
gh workflow run deploy_backend.yaml --ref main

## Minimal-set comparison

V11 preserves stored IPA. Comparison uses NFD and ignores explicit mid-tone
macrons, affricate ties, unreleased-stop and nonsyllabic annotations, and syllable
separators. Other vowel symbols and diacritics remain significant.

Tone-only sets retain vowel length in their comparison key; length-only sets
retain tone and its position. Mixed groups are split into both kinds of set,
and a word can belong to more than one set. Equivalent pronunciations alone
do not form a set. The hand-maintained sets are unchanged.

V12 reconciles the word deduplication, missing คาว dependencies, and verified
IPA corrections. Already-corrected databases retain their existing IDs and
links. Flyway applies and records both revisions during backend deployment;
do not apply them separately to the live database.
