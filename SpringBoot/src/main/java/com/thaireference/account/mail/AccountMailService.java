package com.thaireference.account.mail;

import com.thaireference.config.properties.AccountMailProperties;
import java.util.List;
import java.util.Map;
import org.springframework.http.MediaType;
import org.springframework.stereotype.Component;
import org.springframework.web.client.RestClient;
import org.springframework.web.client.RestClientException;

@Component
public class AccountMailService implements AccountMailPort {

    private final RestClient restClient = RestClient.create("https://api.resend.com");
    private final AccountMailProperties mailProperties;

    public AccountMailService(AccountMailProperties mailProperties) {
        this.mailProperties = mailProperties;
    }

    @Override
    public void send(String to, String subject, String body) {
        String apiKey = mailProperties.resendApiKey();
        if (apiKey == null || apiKey.isBlank()) {
            throw new IllegalStateException("Resend API key is not configured");
        }

        try {
            restClient
                    .post()
                    .uri("/emails")
                    .contentType(MediaType.APPLICATION_JSON)
                    .header("Authorization", "Bearer " + apiKey)
                    .body(Map.of(
                            "from", mailProperties.from(),
                            "to", List.of(to),
                            "subject", subject,
                            "text", body))
                    .retrieve()
                    .toBodilessEntity();
        } catch (RestClientException ex) {
            throw new IllegalStateException("Resend email request failed", ex);
        }
    }
}
