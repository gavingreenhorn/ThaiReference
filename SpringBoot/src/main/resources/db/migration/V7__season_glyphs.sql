ALTER TABLE thai."Season" ADD COLUMN glyph text;

ALTER TABLE thai."Season" ALTER COLUMN glyph SET NOT NULL;
