package com.thaireference.security;

import com.thaireference.util.JsonErrors;
import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;
import java.io.IOException;
import lombok.RequiredArgsConstructor;
import org.springframework.http.HttpHeaders;
import org.springframework.http.HttpStatus;
import org.springframework.lang.NonNull;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.core.Authentication;
import org.springframework.security.core.AuthenticationException;
import org.springframework.security.core.context.SecurityContext;
import org.springframework.security.core.context.SecurityContextHolder;
import org.springframework.security.web.util.matcher.AndRequestMatcher;
import org.springframework.security.web.util.matcher.AntPathRequestMatcher;
import org.springframework.security.web.util.matcher.NegatedRequestMatcher;
import org.springframework.security.web.util.matcher.RequestMatcher;
import org.springframework.web.filter.OncePerRequestFilter;

@RequiredArgsConstructor
public class DeviceUuidAuthenticationFilter extends OncePerRequestFilter {
    private final AuthenticationManager authenticationManager;

    private static final RequestMatcher MATCHER = new AndRequestMatcher(
        new AntPathRequestMatcher("/account/**"),
        new NegatedRequestMatcher(new AntPathRequestMatcher("/account/checkin", "POST"))
    );

    @Override
    protected boolean shouldNotFilter(@NonNull HttpServletRequest request) {
        return !MATCHER.matches(request);
    }

    @Override
    protected void doFilterInternal(
        @NonNull HttpServletRequest request,
        @NonNull HttpServletResponse response,
        @NonNull FilterChain filterChain
    ) throws ServletException, IOException
    {
        try {
            DeviceUuidAuthenticationToken token = DeviceUuidAuthenticationToken.getUnauthenticatedToken(
                request.getHeader(HttpHeaders.AUTHORIZATION)
            );
            Authentication authentication = authenticationManager.authenticate(token);
            SecurityContext context = SecurityContextHolder.createEmptyContext();
            context.setAuthentication(authentication);
            SecurityContextHolder.setContext(context);
            filterChain.doFilter(request, response);
        }
        catch (AuthenticationException ex) {
            SecurityContextHolder.clearContext();
            JsonErrors.write(response, HttpStatus.UNAUTHORIZED, "unauthorized");
        }
    }
}
