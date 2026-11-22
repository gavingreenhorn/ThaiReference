ALTER TABLE thai."Class" RENAME COLUMN name TO code;

ALTER TABLE thai."Class"
    ADD COLUMN name integer,
    ADD COLUMN mediapath text,
    ADD COLUMN english_label text;

ALTER TABLE thai."Class"
    ADD CONSTRAINT "Class_name_fkey"
    FOREIGN KEY (name) REFERENCES thai."CompositeName"(id);

CREATE UNIQUE INDEX IF NOT EXISTS uq_thai_class_code ON thai."Class"(code);
