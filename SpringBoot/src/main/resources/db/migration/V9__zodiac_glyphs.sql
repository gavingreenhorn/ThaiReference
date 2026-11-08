ALTER TABLE thai."Zodiac" ADD COLUMN glyph text;

UPDATE thai."Zodiac" SET glyph = U&'\2648\FE0E' WHERE english_name = 'Aries';
UPDATE thai."Zodiac" SET glyph = U&'\2649\FE0E' WHERE english_name = 'Taurus';
UPDATE thai."Zodiac" SET glyph = U&'\264A\FE0E' WHERE english_name = 'Gemini';
UPDATE thai."Zodiac" SET glyph = U&'\264B\FE0E' WHERE english_name = 'Cancer';
UPDATE thai."Zodiac" SET glyph = U&'\264C\FE0E' WHERE english_name = 'Leo';
UPDATE thai."Zodiac" SET glyph = U&'\264D\FE0E' WHERE english_name = 'Virgo';
UPDATE thai."Zodiac" SET glyph = U&'\264E\FE0E' WHERE english_name = 'Libra';
UPDATE thai."Zodiac" SET glyph = U&'\264F\FE0E' WHERE english_name = 'Scorpio';
UPDATE thai."Zodiac" SET glyph = U&'\2650\FE0E' WHERE english_name = 'Sagittarius';
UPDATE thai."Zodiac" SET glyph = U&'\2651\FE0E' WHERE english_name = 'Capricorn';
UPDATE thai."Zodiac" SET glyph = U&'\2652\FE0E' WHERE english_name = 'Aquarius';
UPDATE thai."Zodiac" SET glyph = U&'\2653\FE0E' WHERE english_name = 'Pisces';

ALTER TABLE thai."Zodiac" ALTER COLUMN glyph SET NOT NULL;
