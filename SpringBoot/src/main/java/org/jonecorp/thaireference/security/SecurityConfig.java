package org.jonecorp.thaireference.security;

import org.jonecorp.thaireference.account.repository.DeviceRepository;
import org.jonecorp.thaireference.auth.UserAccountService;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;
import org.springframework.http.HttpMethod;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.authentication.ProviderManager;
import org.springframework.security.authentication.dao.DaoAuthenticationProvider;
import org.springframework.security.config.annotation.web.builders.HttpSecurity;
import org.springframework.security.config.annotation.web.configurers.AbstractAuthenticationFilterConfigurer;
import org.springframework.security.config.annotation.web.configurers.AbstractHttpConfigurer;
import org.springframework.security.config.annotation.web.configurers.LogoutConfigurer;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.security.web.SecurityFilterChain;
import org.springframework.security.web.authentication.UsernamePasswordAuthenticationFilter;
import org.springframework.security.web.util.matcher.AntPathRequestMatcher;

@Configuration
public class SecurityConfig {

    @Bean
    AuthenticationManager authenticationManager(
            UserAccountService userAccountService,
            DeviceRepository deviceRepository,
            PasswordEncoder passwordEncoder) {
        DaoAuthenticationProvider daoProvider = new DaoAuthenticationProvider(passwordEncoder);
        daoProvider.setUserDetailsService(userAccountService);
        return new ProviderManager(daoProvider, new DeviceUuidAuthenticationProvider(deviceRepository));
    }

    @Bean
    SecurityFilterChain filterChain(
            HttpSecurity http,
            AuthenticationManager authenticationManager)
            throws Exception {
        return http.authenticationManager(authenticationManager)
                .csrf(AbstractHttpConfigurer::disable)
                .authorizeHttpRequests(
                    auth -> auth.requestMatchers(
                        AntPathRequestMatcher.antMatcher("/admin/**")).authenticated()
                        .requestMatchers(HttpMethod.POST, "/account/checkin").permitAll()
                        .requestMatchers("/account/**").authenticated()
                        .anyRequest().permitAll()
                )
                .formLogin(AbstractAuthenticationFilterConfigurer::permitAll).logout(LogoutConfigurer::permitAll)
                .addFilterBefore(
                        new DeviceUuidAuthenticationFilter(authenticationManager),
                        UsernamePasswordAuthenticationFilter.class)
                .build();
    }

    @Bean
    PasswordEncoder passwordEncoder() {
        return new BCryptPasswordEncoder();
    }
}
