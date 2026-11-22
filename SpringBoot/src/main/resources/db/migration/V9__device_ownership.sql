ALTER TABLE thai.device ADD COLUMN model character varying(128);

ALTER TABLE thai.device_group ADD COLUMN owner_device_id bigint;

ALTER TABLE thai.device_group
    ADD CONSTRAINT fk_device_group_owner FOREIGN KEY (owner_device_id) REFERENCES thai.device (id);
