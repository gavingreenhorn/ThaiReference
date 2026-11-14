package org.jonecorp.thaireference.snapadmin;

import jakarta.persistence.EntityManager;
import jakarta.persistence.PersistenceContext;
import org.springframework.boot.ApplicationArguments;
import org.springframework.boot.ApplicationRunner;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.core.annotation.Order;
import org.springframework.stereotype.Component;
import tech.ailef.snapadmin.external.SnapAdmin;
import tech.ailef.snapadmin.external.dbmapping.DbObjectSchema;

/** Swaps SnapAdmin's per-schema repository for {@link FixedCustomJpaRepository} after entity scan. */
@Component
@Order(0)
@ConditionalOnProperty(name = "snapadmin.enabled", havingValue = "true")
public class SnapAdminQueryFixRunner implements ApplicationRunner {

    private final SnapAdmin snapAdmin;

    @PersistenceContext
    private EntityManager entityManager;

    public SnapAdminQueryFixRunner(SnapAdmin snapAdmin) {
        this.snapAdmin = snapAdmin;
    }

    @Override
    public void run(ApplicationArguments args) {
        for (DbObjectSchema schema : snapAdmin.getSchemas()) {
            schema.setJpaRepository(new FixedCustomJpaRepository(schema, entityManager));
        }
    }
}
