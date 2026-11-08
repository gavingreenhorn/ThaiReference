ALTER TABLE thai."Season" ADD COLUMN glyph text;

UPDATE thai."Season" SET glyph = '❄' WHERE ordinal = 1;
UPDATE thai."Season" SET glyph = '☀' WHERE ordinal = 2;
UPDATE thai."Season" SET glyph = '☔' WHERE ordinal = 3;

ALTER TABLE thai."Season" ALTER COLUMN glyph SET NOT NULL;
