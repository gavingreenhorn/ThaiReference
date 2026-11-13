ALTER TABLE thai.device ADD COLUMN model character varying(128);
ALTER TABLE thai.device_group ADD COLUMN owner_device_id bigint;

UPDATE thai.device_group g
SET owner_device_id = (
    SELECT d.id FROM thai.device d
    WHERE d.device_group_id = g.id
    ORDER BY d.linked_at ASC NULLS LAST, d.id ASC LIMIT 1
);

ALTER TABLE thai.device_group
    ADD CONSTRAINT fk_device_group_owner FOREIGN KEY (owner_device_id) REFERENCES thai.device (id);
