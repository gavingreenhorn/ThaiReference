package com.thaireference.account.mail;

/** Optional hook for tests to capture mailed codes. Production has no bean. */
public interface MailCodeSink {
    void record(String code);
}
