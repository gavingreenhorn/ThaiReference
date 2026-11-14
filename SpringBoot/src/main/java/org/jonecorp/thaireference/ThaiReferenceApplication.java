package org.jonecorp.thaireference;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.ImportAutoConfiguration;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.autoconfigure.domain.EntityScan;
import org.springframework.cache.annotation.EnableCaching;
import org.springframework.data.jpa.repository.config.EnableJpaRepositories;
import org.springframework.scheduling.annotation.EnableScheduling;
import tech.ailef.snapadmin.external.SnapAdminAutoConfiguration;


@SpringBootApplication
@EnableCaching
@EnableScheduling
@EntityScan({
    "org.jonecorp.thaireference.entity",
    "org.jonecorp.thaireference.view",
    "org.jonecorp.thaireference.auth",
    "org.jonecorp.thaireference.account.entity"
})
@EnableJpaRepositories({
    "org.jonecorp.thaireference.repository",
    "org.jonecorp.thaireference.auth",
    "org.jonecorp.thaireference.account.repository"
})
@ImportAutoConfiguration(SnapAdminAutoConfiguration.class)
public class ThaiReferenceApplication {

    public static void main(String[] args) {
        SpringApplication.run(ThaiReferenceApplication.class, args);
    }
}
