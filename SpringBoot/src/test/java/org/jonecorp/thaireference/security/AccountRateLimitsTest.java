package org.jonecorp.thaireference.security;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatCode;
import static org.assertj.core.api.Assertions.assertThatThrownBy;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

import org.jonecorp.thaireference.config.properties.AccountProperties;
import org.jonecorp.thaireference.exception.AccountException;
import java.time.Duration;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.http.HttpStatus;
import org.springframework.scheduling.TaskScheduler;

class AccountRateLimitsTest {

    private AccountRateLimits limits;

    @BeforeEach
    void setUp() {
        TaskScheduler scheduler = mock(TaskScheduler.class);
        when(scheduler.scheduleAtFixedRate(any(Runnable.class), any(Duration.class))).thenReturn(null);
        limits = new AccountRateLimits(
                new AccountProperties(
                        Duration.ofMinutes(5),
                        Duration.ofHours(24),
                        3,
                        Duration.ofHours(1),
                        3,
                        Duration.ofHours(1),
                        5),
                scheduler);
    }

    @Test
    void fourthCreateFromSameIpIsRateLimited() {
        limits.requireCreate("203.0.113.10");
        limits.requireCreate("203.0.113.10");
        limits.requireCreate("203.0.113.10");
        assertThatThrownBy(() -> limits.requireCreate("203.0.113.10"))
                .isInstanceOf(AccountException.class)
                .extracting(ex -> ((AccountException) ex).getStatus())
                .isEqualTo(HttpStatus.TOO_MANY_REQUESTS);
        limits.requireCreate("198.51.100.1");
    }

    @Test
    void blankIpDoesNotCountForCreates() {
        limits.requireCreate(null);
        limits.requireCreate(" ");
        limits.requireCreate("203.0.113.10");
        limits.requireCreate("203.0.113.10");
        limits.requireCreate("203.0.113.10");
        assertThatThrownBy(() -> limits.requireCreate("203.0.113.10")).isInstanceOf(AccountException.class);
    }

    @Test
    void clearCreatesFreesSlots() {
        limits.requireCreate("203.0.113.10");
        limits.requireCreate("203.0.113.10");
        limits.requireCreate("203.0.113.10");
        limits.clearCreates();
        assertThatCode(() -> limits.requireCreate("203.0.113.10")).doesNotThrowAnyException();
    }

    @Test
    void fourthRequestFromSameIpIsRejectedUntilClear() {
        assertThat(limits.tryConsumeRequest("203.0.113.10")).isTrue();
        assertThat(limits.tryConsumeRequest("203.0.113.10")).isTrue();
        assertThat(limits.tryConsumeRequest("203.0.113.10")).isTrue();
        assertThat(limits.tryConsumeRequest("203.0.113.10")).isFalse();
        limits.clearRequestHits();
        assertThat(limits.tryConsumeRequest("203.0.113.10")).isTrue();
    }

    @Test
    void blankIpDoesNotCountForRequests() {
        assertThat(limits.tryConsumeRequest(null)).isTrue();
        assertThat(limits.tryConsumeRequest(" ")).isTrue();
    }
}
