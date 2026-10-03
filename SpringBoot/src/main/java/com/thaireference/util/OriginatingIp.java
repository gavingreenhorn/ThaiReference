package com.thaireference.util;

import jakarta.servlet.http.HttpServletRequest;
import java.net.InetAddress;
import lombok.experimental.UtilityClass;

@UtilityClass
public class OriginatingIp {

    public static String resolve(HttpServletRequest request) {
        String forwarded = request.getHeader("X-Forwarded-For");
        if (forwarded != null && !forwarded.isBlank()) {
            return validate(forwarded.split(",")[0].strip());
        }
        String realIp = request.getHeader("X-Real-IP");
        if (realIp != null && !realIp.isBlank()) {
            return validate(realIp);
        }
        return validate(request.getRemoteAddr());
    }

    static String validate(String input) {
        try {
            return InetAddress.ofLiteral(input.strip()).getHostAddress();
        } catch (IllegalArgumentException | NullPointerException ex) {
            return null;
        }
    }
}
