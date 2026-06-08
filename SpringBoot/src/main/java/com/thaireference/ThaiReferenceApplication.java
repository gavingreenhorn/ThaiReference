package com.thaireference;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.autoconfigure.domain.EntityScan;
import org.springframework.data.jpa.repository.config.EnableJpaRepositories;

@SpringBootApplication
@EntityScan("com.thaireference.entity")
@EnableJpaRepositories("com.thaireference.repository")
public class ThaiReferenceApplication {

    public static void main(String[] args) {
        SpringApplication.run(ThaiReferenceApplication.class, args);
    }
}
