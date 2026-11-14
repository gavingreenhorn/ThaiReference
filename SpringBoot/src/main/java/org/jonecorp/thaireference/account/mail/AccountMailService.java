package org.jonecorp.thaireference.account.mail;

import org.jonecorp.thaireference.config.properties.AccountMailProperties;
import java.util.List;
import java.util.Map;
import lombok.RequiredArgsConstructor;
import org.springframework.http.MediaType;
import org.springframework.stereotype.Component;
import org.springframework.web.client.RestClient;
import org.springframework.web.client.RestClientException;

@Component
@RequiredArgsConstructor
public class AccountMailService implements AccountMailPort {

    private final RestClient restClient = RestClient.create("https://api.resend.com");
    private final AccountMailProperties mailProperties;

    @Override
    public void sendEmailConfirmation(String to, String code) {
        sendRequest(Map.of(
                "to", List.of(to),
                "template", Map.of(
                        "id", "verification-code",
                        "variables", Map.of(
                                "TITLE", "Please find your email confirmation code below.",
                                "INSTRUCTIONS", "Enter this code in the app. Keep it private and do not share it with anyone.",
                                "CODE", code))));
    }

    @Override
    public void send(String to, String subject, String body) {
        sendRequest(Map.of(
                "from", mailProperties.from(),
                "to", List.of(to),
                "subject", subject,
                "text", body));
    }

    private void sendRequest(Map<String, Object> request) {
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
                    .body(request)
                    .retrieve()
                    .toBodilessEntity();
        } catch (RestClientException ex) {
            throw new IllegalStateException("Resend email request failed", ex);
        }
    }
}
