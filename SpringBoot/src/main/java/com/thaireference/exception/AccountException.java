package com.thaireference.exception;

import lombok.Getter;
import org.springframework.http.HttpStatus;

@Getter
public class AccountException extends RuntimeException {

    private final String error;
    private final HttpStatus status;

    public AccountException(String error, HttpStatus status) {
        super(error);
        this.error = error;
        this.status = status;
    }
}
