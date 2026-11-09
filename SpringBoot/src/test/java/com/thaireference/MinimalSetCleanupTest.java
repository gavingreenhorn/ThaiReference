package com.thaireference;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertThrows;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;
import org.junit.jupiter.api.Test;
import org.springframework.core.io.ClassPathResource;
import org.springframework.jdbc.datasource.init.ScriptUtils;
import org.testcontainers.containers.PostgreSQLContainer;
import org.testcontainers.junit.jupiter.Container;
import org.testcontainers.junit.jupiter.Testcontainers;

@Testcontainers(disabledWithoutDocker = true)
class MinimalSetCleanupTest {
    @Container
    static final PostgreSQLContainer<?> postgres = new PostgreSQLContainer<>("postgres:16-alpine");

    @Test
    void cleanupPreservesReferencesAndDistinctSensesAndIsRepeatable() throws Exception {
        try (Connection connection = DriverManager.getConnection(
                postgres.getJdbcUrl(), postgres.getUsername(), postgres.getPassword())) {
            connection.setAutoCommit(false);
            ScriptUtils.executeSqlScript(connection,
                    new ClassPathResource("sql/minimal_set_cleanup_fixture.sql"));
            String migration = new ClassPathResource(
                    "db/migration/V14__minimal_set_word_and_lexeme_cleanup.sql")
                    .getContentAsString(java.nio.charset.StandardCharsets.UTF_8);
            try (var statement = connection.createStatement()) {
                statement.execute(migration);
                statement.execute(migration);
            }
            assertEquals(30, scalar(connection, "SELECT count(*) FROM thai.\"Word\""));
            assertEquals(2, scalar(connection,
                    "SELECT count(*) FROM thai.\"SentenceWord\" WHERE word_id=157"));
            assertEquals(1, scalar(connection,
                    "SELECT count(*) FROM thai.\"WordLexeme\" WHERE word=157"));
            assertEquals(2, scalar(connection,
                    "SELECT count(*) FROM thai.\"WordLexeme\" WHERE word=29"));
            assertEquals(2, scalar(connection,
                    "SELECT count(*) FROM thai.\"WordLexeme\" WHERE word=162"));
            assertEquals(2, scalar(connection,
                    "SELECT count(*) FROM thai.\"WordLexeme\" WHERE word=376"));
            assertEquals(2, scalar(connection,
                    "SELECT count(*) FROM thai.\"WordLexeme\" WHERE word=205"));
            assertEquals(3, scalar(connection,
                    "SELECT count(*) FROM thai.\"WordLexeme\" WHERE word=556"));
            assertEquals(0, scalar(connection,
                    "SELECT count(*) FROM thai.\"Word\" w WHERE NOT EXISTS "
                    + "(SELECT 1 FROM thai.\"WordLexeme\" wl WHERE wl.word=w.id)"));
            assertEquals(0, scalar(connection,
                    "SELECT count(*) FROM (SELECT word,lexeme FROM thai.\"WordLexeme\" "
                    + "GROUP BY word,lexeme HAVING count(*)>1) d"));
            var savepoint = connection.setSavepoint();
            try (var statement = connection.createStatement()) {
                statement.executeUpdate("UPDATE thai.\"Spelling\" SET value='wrong' WHERE id=157");
                assertThrows(SQLException.class, () -> statement.execute(migration));
            }
            connection.rollback(savepoint);
            connection.rollback();
        }
    }

    private static int scalar(Connection connection, String sql) throws SQLException {
        try (var statement = connection.createStatement(); var result = statement.executeQuery(sql)) {
            result.next();
            return result.getInt(1);
        }
    }
}
