package com.thaireference.util;

import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;
import java.security.SecureRandom;
import java.util.Base64;
import java.util.HexFormat;
import lombok.experimental.UtilityClass;

@UtilityClass
public class TokenHashes {
    private static final SecureRandom RANDOM = new SecureRandom();
    private static final int RAW_TOKEN_BYTES = 32;

    public static String newRawToken() {
        byte[] bytes = new byte[RAW_TOKEN_BYTES];
        RANDOM.nextBytes(bytes);
        return Base64.getUrlEncoder().withoutPadding().encodeToString(bytes);
    }

    public static String newSixDigitCode() {
        return String.format("%06d", RANDOM.nextInt(1_000_000));
    }

    public static String sha256(String raw) {
        try {
            MessageDigest digest = MessageDigest.getInstance("SHA-256");
            return HexFormat.of().formatHex(digest.digest(raw.getBytes(StandardCharsets.UTF_8)));
        } catch (NoSuchAlgorithmException ex) {
            throw new IllegalStateException(ex);
        }
    }
}
