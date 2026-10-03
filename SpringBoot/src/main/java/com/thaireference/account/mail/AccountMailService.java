package com.thaireference.account.mail;

import com.thaireference.config.properties.AccountMailProperties;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.ObjectProvider;
import org.springframework.mail.SimpleMailMessage;
import org.springframework.mail.javamail.JavaMailSender;
import org.springframework.stereotype.Component;

@Component
public class AccountMailService implements AccountMailPort {

    private static final Logger log = LoggerFactory.getLogger(AccountMailService.class);

    private final ObjectProvider<JavaMailSender> mailSender;
    private final AccountMailProperties mailProperties;

    public AccountMailService(
            ObjectProvider<JavaMailSender> mailSender, AccountMailProperties mailProperties) {
        this.mailSender = mailSender;
        this.mailProperties = mailProperties;
    }

    @Override
    public void send(String to, String subject, String body) {
        JavaMailSender sender = mailSender.getIfAvailable();
        if (sender != null) {
            try {
                SimpleMailMessage message = new SimpleMailMessage();
                message.setFrom(mailProperties.from());
                message.setTo(to);
                message.setSubject(subject);
                message.setText(body);
                sender.send(message);
                return;
            } catch (Exception ex) {
                log.warn("SMTP send failed; logging mail body instead", ex);
            }
        }
        log.info("Mail for {} ({}): {}", to, subject, body);
    }
}
