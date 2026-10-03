package com.thaireference.account.mail;

import java.util.concurrent.atomic.AtomicReference;
import org.springframework.stereotype.Component;

@Component
public class MailCodeRecorder implements MailCodeSink {

    private final AtomicReference<String> lastCode = new AtomicReference<>();

    @Override
    public void record(String code) {
        lastCode.set(code);
    }

    public String lastCode() {
        return lastCode.get();
    }
}
