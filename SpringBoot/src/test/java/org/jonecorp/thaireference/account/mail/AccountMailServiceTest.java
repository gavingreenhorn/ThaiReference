package org.jonecorp.thaireference.account.mail;

import static org.assertj.core.api.Assertions.assertThatThrownBy;
import static org.mockito.Mockito.mockStatic;
import static org.springframework.test.web.client.match.MockRestRequestMatchers.content;
import static org.springframework.test.web.client.match.MockRestRequestMatchers.header;
import static org.springframework.test.web.client.match.MockRestRequestMatchers.method;
import static org.springframework.test.web.client.match.MockRestRequestMatchers.requestTo;
import static org.springframework.test.web.client.response.MockRestResponseCreators.withBadRequest;
import static org.springframework.test.web.client.response.MockRestResponseCreators.withSuccess;

import org.jonecorp.thaireference.config.properties.AccountMailProperties;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.MockedStatic;
import org.springframework.http.HttpMethod;
import org.springframework.http.MediaType;
import org.springframework.test.web.client.MockRestServiceServer;
import org.springframework.web.client.RestClient;
import org.springframework.web.client.RestClientException;

class AccountMailServiceTest {

    private MockRestServiceServer server;
    private AccountMailService mail;

    @BeforeEach
    void setUp() {
        RestClient.Builder builder = RestClient.builder().baseUrl("https://api.resend.com");
        server = MockRestServiceServer.bindTo(builder).build();
        RestClient client = builder.build();
        try (MockedStatic<RestClient> restClients = mockStatic(RestClient.class)) {
            restClients.when(() -> RestClient.create("https://api.resend.com")).thenReturn(client);
            mail = new AccountMailService(new AccountMailProperties("sender@example.com", "test-api-key"));
        }
    }

    @Test
    void confirmationUsesTemplateAndPreservesLeadingZero() {
        server.expect(requestTo("https://api.resend.com/emails"))
                .andExpect(method(HttpMethod.POST))
                .andExpect(header("Authorization", "Bearer test-api-key"))
                .andExpect(content().contentType(MediaType.APPLICATION_JSON))
                .andExpect(content().json("""
                        {
                          "to": ["recipient@example.com"],
                          "template": {
                            "id": "verification-code",
                            "variables": {
                              "TITLE": "Please find your email confirmation code below.",
                              "INSTRUCTIONS": "Enter this code in the app. Keep it private and do not share it with anyone.",
                              "CODE": "012345"
                            }
                          }
                        }
                        """, true))
                .andRespond(withSuccess());

        mail.sendEmailConfirmation("recipient@example.com", "012345");

        server.verify();
    }

    @Test
    void recoveryKeepsPlainTextWithoutTemplate() {
        server.expect(requestTo("https://api.resend.com/emails"))
                .andExpect(content().json("""
                        {
                          "from": "sender@example.com",
                          "to": ["recipient@example.com"],
                          "subject": "Recover your devices",
                          "text": "Your code is: 012345"
                        }
                        """, true))
                .andRespond(withSuccess());

        mail.send("recipient@example.com", "Recover your devices", "Your code is: 012345");

        server.verify();
    }

    @Test
    void templateRejectionIsReportedWithoutPlainTextFallback() {
        server.expect(requestTo("https://api.resend.com/emails"))
                .andRespond(withBadRequest());

        assertThatThrownBy(() -> mail.sendEmailConfirmation("recipient@example.com", "012345"))
                .isInstanceOf(IllegalStateException.class)
                .hasMessage("Resend email request failed")
                .hasCauseInstanceOf(RestClientException.class);

        server.verify();
    }
}
