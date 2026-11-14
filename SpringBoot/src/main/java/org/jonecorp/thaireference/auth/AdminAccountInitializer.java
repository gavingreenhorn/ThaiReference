package org.jonecorp.thaireference.auth;

import org.jonecorp.thaireference.config.properties.AppProperties;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.boot.ApplicationArguments;
import org.springframework.boot.ApplicationRunner;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

/**
 * Ensures the admin account exists, seeded from {@code app.admin.*} (env ADMIN_USERNAME /
 * ADMIN_PASSWORD). Env stays the source of truth: if the env password no longer matches the stored
 * hash it is re-hashed and saved, so rotating the env rotates the credential. Skips silently when
 * credentials are not configured (e.g. tests), leaving the panel closed.
 */
@Component
public class AdminAccountInitializer implements ApplicationRunner {

    private static final Logger log = LoggerFactory.getLogger(AdminAccountInitializer.class);

    private final UserAccountRepository repository;
    private final PasswordEncoder passwordEncoder;
    private final AppProperties appProperties;

    public AdminAccountInitializer(
            UserAccountRepository repository, PasswordEncoder passwordEncoder, AppProperties appProperties) {
        this.repository = repository;
        this.passwordEncoder = passwordEncoder;
        this.appProperties = appProperties;
    }

    @Override
    @Transactional
    public void run(ApplicationArguments args) {
        AppProperties.Admin admin = appProperties.admin();
        if (admin == null
                || admin.username() == null || admin.username().isBlank()
                || admin.password() == null || admin.password().isBlank()) {
            log.info("No admin credentials configured (app.admin.*); skipping admin account bootstrap.");
            return;
        }

        UserAccount existing = repository.findByUsername(admin.username()).orElse(null);
        if (existing == null) {
            repository.save(new UserAccount(admin.username(), passwordEncoder.encode(admin.password()), Role.ADMIN));
            log.info("Created admin account '{}'.", admin.username());
        } else if (!passwordEncoder.matches(admin.password(), existing.getPassword())) {
            existing.setPassword(passwordEncoder.encode(admin.password()));
            repository.save(existing);
            log.info("Rotated admin account '{}' password from env.", admin.username());
        }
    }
}
