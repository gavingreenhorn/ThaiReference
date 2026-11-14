CREATE SCHEMA account;

ALTER TABLE thai.user_accounts SET SCHEMA account;
ALTER TABLE thai.device_group SET SCHEMA account;
ALTER TABLE thai.device SET SCHEMA account;
ALTER TABLE thai.account_token SET SCHEMA account;
