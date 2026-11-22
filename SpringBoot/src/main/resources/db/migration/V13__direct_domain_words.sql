ALTER TABLE thai."Direction" ADD COLUMN word_id integer REFERENCES thai."Word"(id);

ALTER TABLE thai."Direction" ALTER COLUMN word_id SET NOT NULL, DROP COLUMN composite_name_id;

ALTER TABLE thai."Class" ADD COLUMN word_id integer REFERENCES thai."Word"(id);

ALTER TABLE thai."Class" ALTER COLUMN word_id SET NOT NULL, DROP COLUMN name;

ALTER TABLE thai."Season" ADD COLUMN word_id integer REFERENCES thai."Word"(id);

ALTER TABLE thai."Season" ALTER COLUMN word_id SET NOT NULL, DROP COLUMN name;

ALTER TABLE thai."ToneMark" ADD COLUMN word_id integer REFERENCES thai."Word"(id);

ALTER TABLE thai."ToneMark" DROP COLUMN name;

ALTER TABLE thai."Color" ADD COLUMN word_id integer REFERENCES thai."Word"(id);

ALTER TABLE thai."Color" ADD CONSTRAINT "Color_label_reference_check"
    CHECK ((word_id IS NOT NULL) <> (composite_name_id IS NOT NULL));
