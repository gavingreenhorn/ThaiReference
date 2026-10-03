package com.thaireference.account.mail;

public interface AccountMailPort {

    void send(String to, String subject, String body);
}
