package com.thaireference.config;

import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import java.io.IOException;
import org.springframework.core.annotation.Order;
import org.springframework.lang.NonNull;
import org.springframework.stereotype.Component;
import org.springframework.web.filter.OncePerRequestFilter;


@Component
@Order
public class AdminWriteRefreshFilter extends OncePerRequestFilter {

    private static final String ADMIN_PREFIX = "/admin/";

    private final ReferenceDataRefreshService refreshService;

    public AdminWriteRefreshFilter(ReferenceDataRefreshService refreshService) {
        this.refreshService = refreshService;
    }

    @Override
    protected void doFilterInternal(
            @NonNull HttpServletRequest request,
            @NonNull HttpServletResponse response,
            @NonNull FilterChain filterChain)
            throws ServletException, IOException {
        filterChain.doFilter(request, response);
        if (isAdminMutatingRequest(request) && isSuccessful(response)) {
            refreshService.scheduleRefreshAfterAdminWrite();
        }
    }

    private static boolean isAdminMutatingRequest(HttpServletRequest request) {
        if (!request.getRequestURI().startsWith(ADMIN_PREFIX)) {
            return false;
        }
        return switch (request.getMethod()) {
            case "POST", "PUT", "PATCH", "DELETE" -> true;
            default -> false;
        };
    }

    private static boolean isSuccessful(HttpServletResponse response) {
        int status = response.getStatus();
        return status >= 200 && status < 300;
    }
}
