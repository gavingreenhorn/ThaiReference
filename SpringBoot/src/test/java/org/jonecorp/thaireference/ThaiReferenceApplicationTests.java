package org.jonecorp.thaireference;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;

import org.jonecorp.thaireference.entity.CompoundWord;
import org.jonecorp.thaireference.entity.SimpleWord;
import org.jonecorp.thaireference.entity.WordComponent;
import org.jonecorp.thaireference.entity.WordLexeme;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.test.context.TestPropertySource;
import tech.ailef.snapadmin.external.SnapAdmin;
import tech.ailef.snapadmin.external.dbmapping.DbObjectSchema;

@SpringBootTest(webEnvironment = SpringBootTest.WebEnvironment.RANDOM_PORT)
@ActiveProfiles("test")
@TestPropertySource(properties = {"snapadmin.enabled=true", "snapadmin.test-mode=true"})
class ThaiReferenceApplicationTests {

    @Autowired
    private SnapAdmin snapAdmin;

    @Test
    void contextLoads() {}

    @Test
    void snapAdminMapsJoinedWordEntitiesAndInheritedIds() {
        assertTrue(snapAdmin.isManagedClass(WordComponent.class));
        assertTrue(snapAdmin.isManagedClass(WordLexeme.class));

        DbObjectSchema simpleWord = snapAdmin.findSchemaByClass(SimpleWord.class);
        DbObjectSchema compoundWord = snapAdmin.findSchemaByClass(CompoundWord.class);
        assertEquals("id", simpleWord.getPrimaryKey().getJavaName());
        assertEquals("id", compoundWord.getPrimaryKey().getJavaName());
    }
}
