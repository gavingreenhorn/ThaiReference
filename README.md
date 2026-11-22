# ThaiReference

## Reference names

Directions, consonant classes, seasons and named tone marks reference a `Word`
directly. Their API responses contain a singular `word` object, including its
identity, whole-word lexemes and ordered simple-word component IDs. The
no-tone-mark entry has `word: null`. Special-symbol marks use the same contract.

Colors retain the existing `ColorLabel` DTO shape: `words` contains either one
whole word or the ordered words of a retained CompositeName phrase. Color labels
are not themselves words, and constituent lexemes are not whole-label meanings.
The database enforces exactly one word or CompositeName reference per color.
Positions retain phrase support; ท่ามกลาง reuses SimpleWord 532.

V12 and V13 define the polymorphic word schema and direct domain references.
Migration files contain DDL only: word data, pronunciation corrections, domain
backfills and materialized-view refreshes are performed separately.

Historical migrations were cleaned after application. Existing databases need
Flyway `repair` against the cleaned files before deployment; this reconciles
checksums without rerunning those migrations or changing reference data.

Offline snapshots use schema version 2. Rebuild the packaged snapshot with the
existing FetchLiteDb project after deploying the updated backend. Older AppData
snapshots are replaced from the packaged snapshot rather than deserialized as
the new direct-word models.
