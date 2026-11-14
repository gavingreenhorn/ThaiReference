package org.jonecorp.thaireference.account.mail;

import static org.assertj.core.api.Assertions.assertThat;

import java.util.concurrent.atomic.AtomicReference;
import org.springframework.context.annotation.Primary;
import org.springframework.stereotype.Component;

@Component
@Primary
public class MailCodeRecorder implements AccountMailPort {

    private final AtomicReference<String> lastCode = new AtomicReference<>();

    @Override
    public void sendEmailConfirmation(String to, String code) {
        assertThat(code).matches("[0-9]{6}");
        lastCode.set(code);
    }

    @Override
    public void send(String to, String subject, String body) {
        assertThat(body).matches("Your code is: [0-9]{6}");
        lastCode.set(body.substring("Your code is: ".length()));
    }

    public String lastCode() {
        return lastCode.get();
    }
}
