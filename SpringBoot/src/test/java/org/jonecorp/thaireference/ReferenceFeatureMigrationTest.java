package org.jonecorp.thaireference;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertThrows;

import java.sql.DriverManager;
import java.sql.SQLException;
import org.flywaydb.core.Flyway;
import org.junit.jupiter.api.Test;
import org.testcontainers.containers.PostgreSQLContainer;
import org.testcontainers.junit.jupiter.Container;
import org.testcontainers.junit.jupiter.Testcontainers;

@Testcontainers
class ReferenceFeatureMigrationTest {
    @Container
    static final PostgreSQLContainer<?> postgres = new PostgreSQLContainer<>("postgres:16-alpine");

    @Test
    void ddlOnlyMigrationsCreateAndValidateSchemaWithoutSeedingWords() throws Exception {
        try (var connection = DriverManager.getConnection(
                postgres.getJdbcUrl(), postgres.getUsername(), postgres.getPassword());
                var statement = connection.createStatement()) {
            Flyway flyway = Flyway.configure()
                    .dataSource(postgres.getJdbcUrl(), postgres.getUsername(), postgres.getPassword())
                    .schemas("thai")
                    .target("14")
                    .load();
            assertEquals(14, flyway.migrate().migrationsExecuted);
            flyway.validate();
            assertEquals(0, flyway.migrate().migrationsExecuted);
            try (var rows = statement.executeQuery("""
                    SELECT
                      (SELECT count(*) FROM information_schema.tables
                       WHERE table_schema = 'account' AND table_name = 'device_group'),
                      (SELECT count(*) FROM information_schema.tables
                       WHERE table_schema = 'account' AND table_name = 'device'),
                      (SELECT count(*) FROM information_schema.tables
                       WHERE table_schema = 'account' AND table_name = 'account_token'),
                      (SELECT count(*) FROM information_schema.tables
                       WHERE table_schema = 'account' AND table_name = 'user_accounts'),
                      (SELECT count(*) FROM information_schema.tables
                       WHERE table_schema = 'thai' AND table_name IN
                         ('device_group', 'device', 'account_token', 'user_accounts')),
                      (SELECT count(*) FROM pg_constraint c
                       JOIN pg_class t ON t.oid = c.conrelid
                       JOIN pg_namespace n ON n.oid = t.relnamespace
                       WHERE n.nspname = 'account' AND c.contype = 'f')
                    """)) {
                rows.next();
                for (int i = 1; i <= 4; i++) {
                    assertEquals(1, rows.getInt(i));
                }
                assertEquals(0, rows.getInt(5));
                assertEquals(3, rows.getInt(6));
            }
            connection.setAutoCommit(false);
            try (var rows = statement.executeQuery("SELECT count(*) FROM thai.\"Word\"")) {
                rows.next();
                assertEquals(0, rows.getInt(1));
            }
            statement.execute("""
                    INSERT INTO thai."Spelling"(id,value) VALUES (1,'test');
                    INSERT INTO thai."Transcription"(id,paiboon,"IPA") VALUES (1,'test','test');
                    INSERT INTO thai."Word"(id) VALUES (1);
                    INSERT INTO thai."SimpleWord"(id,spelling_id,transcription) VALUES (1,1,1);
                    INSERT INTO thai."Sentence"(id,translation) VALUES (1,'one'), (2,'two'), (3,'three');
                    INSERT INTO thai."SentenceWord"(id,sentence_id,word_id,word_order) VALUES (1,1,1,1);
                    INSERT INTO thai."Mood"(word_id) VALUES (1);
                    INSERT INTO thai."MoodSentence"(mood_id, sentence_id, position)
                    SELECT id, 1, 1 FROM thai."Mood";
                    INSERT INTO thai."MoodSentence"(mood_id, sentence_id, position)
                    SELECT id, 2, 2 FROM thai."Mood";
                    """);
            try (var rows = statement.executeQuery(
                    "SELECT following_text FROM thai.\"SentenceWord\" WHERE id = 1")) {
                rows.next();
                assertEquals("", rows.getString(1));
            }
            statement.execute("""
                    UPDATE thai."Mood" SET svg = '<svg xmlns="http://www.w3.org/2000/svg"/>'
                    WHERE word_id = 1
                    """);
            try (var rows = statement.executeQuery("SELECT svg FROM thai.\"Mood\" WHERE word_id = 1")) {
                rows.next();
                assertEquals("<svg xmlns=\"http://www.w3.org/2000/svg\"/>", rows.getString(1));
            }
            var savepoint = connection.setSavepoint();
            assertThrows(SQLException.class, () -> statement.execute("""
                    INSERT INTO thai."MoodSentence"(mood_id, sentence_id, position)
                    SELECT id, 3, 1 FROM thai."Mood";
                    """));
            connection.rollback(savepoint);
            statement.execute("""
                    INSERT INTO thai."CompositeName"(id,word1,word2) VALUES (1,1,1), (2,1,1);
                    INSERT INTO thai."Position"(id,word_id,sentence_id,lexeme) VALUES (1,1,1,'on');
                    INSERT INTO thai."Position"(id,composite_name_id,sentence_id,lexeme)
                    VALUES (2,1,2,'phrase');
                    """);
            var positionSavepoint = connection.setSavepoint();
            SQLException bothReferences = assertThrows(SQLException.class, () -> statement.execute("""
                    INSERT INTO thai."Position"(id,word_id,composite_name_id,sentence_id,lexeme)
                    VALUES (3,1,2,3,'invalid');
                    """));
            assertEquals("23514", bothReferences.getSQLState());
            connection.rollback(positionSavepoint);
            SQLException neitherReference = assertThrows(SQLException.class, () -> statement.execute("""
                    INSERT INTO thai."Position"(id,sentence_id,lexeme) VALUES (3,3,'invalid');
                    """));
            assertEquals("23514", neitherReference.getSQLState());
            connection.rollback(positionSavepoint);
            assertThrows(SQLException.class, () -> statement.execute("""
                    INSERT INTO thai."MoodSentence"(mood_id, sentence_id, position)
                    SELECT id, 3, 0 FROM thai."Mood";
                    """));
            connection.rollback(savepoint);
            try (var rows = statement.executeQuery("""
                    SELECT count(*) FROM information_schema.columns
                    WHERE table_schema = 'thai' AND table_name = 'MoodSentence'
                      AND column_name IN ('source_url', 'source_author', 'source_license')
                    """)) {
                rows.next();
                assertEquals(0, rows.getInt(1));
            }
            connection.rollback();
        }
    }
}
