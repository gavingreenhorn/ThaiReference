package com.thaireference.account.mail;

import java.util.concurrent.atomic.AtomicReference;
import org.springframework.context.annotation.Primary;
import org.springframework.stereotype.Component;

@Component
@Primary
public class MailCodeRecorder implements AccountMailPort, MailCodeSink {

    private final AtomicReference<String> lastCode = new AtomicReference<>();

    @Override
    public void record(String code) {
        lastCode.set(code);
    }

    @Override
    public void send(String to, String subject, String body) {}

    public String lastCode() {
        return lastCode.get();
    }
}
