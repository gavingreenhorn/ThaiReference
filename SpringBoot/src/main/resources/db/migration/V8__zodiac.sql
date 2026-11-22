ALTER TABLE thai."Zodiac" ADD COLUMN glyph text;

ALTER TABLE thai."Zodiac" ALTER COLUMN glyph SET NOT NULL;

ALTER TABLE thai."Zodiac" DROP COLUMN image_url;
