ALTER TABLE thai."Lexeme"
    ADD CONSTRAINT uq_thai_lexeme_value_part_of_speech
    UNIQUE NULLS NOT DISTINCT (value, part_of_speech);
