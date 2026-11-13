package com.thaireference;

import static org.junit.jupiter.api.Assertions.assertEquals;

import java.nio.charset.StandardCharsets;
import java.sql.DriverManager;
import org.flywaydb.core.Flyway;
import org.junit.jupiter.api.Test;
import org.springframework.core.io.ClassPathResource;
import org.testcontainers.containers.PostgreSQLContainer;
import org.testcontainers.junit.jupiter.Container;
import org.testcontainers.junit.jupiter.Testcontainers;

@Testcontainers
class ReferenceFeatureMigrationTest {
    @Container
    static final PostgreSQLContainer<?> postgres = new PostgreSQLContainer<>("postgres:16-alpine");

    @Test
    void consolidatedFeaturesMigrateAndValidateWithoutWordDataHotfixes() throws Exception {
        try (var connection = DriverManager.getConnection(
                postgres.getJdbcUrl(), postgres.getUsername(), postgres.getPassword());
                var statement = connection.createStatement()) {
            statement.execute("""
                    CREATE SCHEMA thai;
                    CREATE TABLE thai."Word"(id integer, transcription integer, is_primary boolean);
                    CREATE TABLE thai."Transcription"(id integer, "IPA" text);
                    CREATE TABLE thai."Sound"(
                        id integer, value text, consonant_sound integer, initial_only boolean);
                    CREATE TABLE thai."Season"(id integer, ordinal integer);
                    INSERT INTO thai."Season" VALUES (1,1),(2,2),(3,3);
                    CREATE TABLE thai."Zodiac"(id integer, english_name text, image_url text);
                    CREATE TABLE thai.device_group(id bigint PRIMARY KEY);
                    CREATE TABLE thai.device(
                        id bigint PRIMARY KEY, device_group_id bigint, linked_at timestamptz);
                    INSERT INTO thai."Zodiac" VALUES
                        (1,'Aries',NULL),(2,'Taurus',NULL),(3,'Gemini',NULL),
                        (4,'Cancer',NULL),(5,'Leo',NULL),(6,'Virgo',NULL),
                        (7,'Libra',NULL),(8,'Scorpio',NULL),(9,'Sagittarius',NULL),
                        (10,'Capricorn',NULL),(11,'Aquarius',NULL),(12,'Pisces',NULL);
                    """);
            Flyway flyway = Flyway.configure()
                    .dataSource(postgres.getJdbcUrl(), postgres.getUsername(), postgres.getPassword())
                    .schemas("thai")
                    .baselineOnMigrate(true)
                    .baselineVersion("5")
                    .load();
            assertEquals(4, flyway.migrate().migrationsExecuted);
            flyway.validate();
            assertEquals(0, flyway.migrate().migrationsExecuted);
            connection.setAutoCommit(false);
            statement.execute(new ClassPathResource("sql/minimal_sets_ipa_comparison.sql")
                    .getContentAsString(StandardCharsets.UTF_8));
            connection.rollback();
            try (var rows = statement.executeQuery("SELECT count(*) FROM thai.\"Word\"")) {
                rows.next();
                assertEquals(0, rows.getInt(1));
            }
        }
    }
}
