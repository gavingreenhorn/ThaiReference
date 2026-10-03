package com.thaireference.exception;

import com.thaireference.account.web.AccountController;
import java.util.Map;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.ExceptionHandler;
import org.springframework.web.bind.annotation.RestControllerAdvice;

@RestControllerAdvice(assignableTypes = AccountController.class)
public class AccountExceptionHandler {

    @ExceptionHandler(AccountException.class)
    public ResponseEntity<Map<String, String>> handle(AccountException ex) {
        return ResponseEntity.status(ex.getStatus()).body(Map.of("error", ex.getError()));
    }
}
