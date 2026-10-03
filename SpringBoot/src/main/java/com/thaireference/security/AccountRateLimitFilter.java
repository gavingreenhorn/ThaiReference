package com.thaireference.security;

import com.thaireference.util.JsonErrors;
import com.thaireference.util.OriginatingIp;
import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import java.io.IOException;
import org.springframework.http.HttpStatus;
import org.springframework.lang.NonNull;
import org.springframework.security.web.util.matcher.AntPathRequestMatcher;
import org.springframework.security.web.util.matcher.OrRequestMatcher;
import org.springframework.security.web.util.matcher.RequestMatcher;
import org.springframework.stereotype.Component;
import org.springframework.web.filter.OncePerRequestFilter;

@Component
public class AccountRateLimitFilter extends OncePerRequestFilter {

    private static final RequestMatcher LIMITED = new OrRequestMatcher(
            new AntPathRequestMatcher("/account/checkin", "POST"),
            new AntPathRequestMatcher("/account/recover", "POST"));

    private final AccountRateLimits rateLimits;

    public AccountRateLimitFilter(AccountRateLimits rateLimits) {
        this.rateLimits = rateLimits;
    }

    @Override
    protected boolean shouldNotFilter(@NonNull HttpServletRequest request) {
        return !LIMITED.matches(request);
    }

    @Override
    protected void doFilterInternal(
            @NonNull HttpServletRequest request,
            @NonNull HttpServletResponse response,
            @NonNull FilterChain filterChain)
            throws ServletException, IOException {
        if (!rateLimits.tryConsumeRequest(OriginatingIp.resolve(request))) {
            JsonErrors.write(response, HttpStatus.TOO_MANY_REQUESTS, "no");
            return;
        }
        filterChain.doFilter(request, response);
    }
}
