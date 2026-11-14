package org.jonecorp.thaireference.account.mail;

public interface AccountMailPort {

    void sendEmailConfirmation(String to, String code);

    void send(String to, String subject, String body);
}
