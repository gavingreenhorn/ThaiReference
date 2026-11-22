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

V13 applies these references and the approved word conversions without changing
existing word IDs or whole-word lexeme links. V12 remains unchanged.

Offline snapshots use schema version 2. Rebuild the packaged snapshot with the
existing FetchLiteDb project after deploying the updated backend. Older AppData
snapshots are replaced from the packaged snapshot rather than deserialized as
the new direct-word models.
