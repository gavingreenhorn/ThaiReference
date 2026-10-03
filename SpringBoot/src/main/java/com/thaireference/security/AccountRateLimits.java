package com.thaireference.security;

import com.thaireference.config.properties.AccountProperties;
import com.thaireference.exception.AccountException;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicInteger;
import org.springframework.http.HttpStatus;
import org.springframework.scheduling.TaskScheduler;
import org.springframework.stereotype.Component;

@Component
public class AccountRateLimits {

    private final ConcurrentHashMap<String, AtomicInteger> requestHits = new ConcurrentHashMap<>();
    private final ConcurrentHashMap<String, AtomicInteger> creates = new ConcurrentHashMap<>();
    private final AccountProperties accountProperties;

    public AccountRateLimits(AccountProperties accountProperties, TaskScheduler taskScheduler)
    {
        this.accountProperties = accountProperties;
        taskScheduler.scheduleAtFixedRate(requestHits::clear, accountProperties.rateLimitWindow());
        taskScheduler.scheduleAtFixedRate(creates::clear, accountProperties.createQuotaWindow());
    }

    public boolean tryConsumeRequest(String ip)
    {
        if (ip == null || ip.isBlank())
            return true;
        return requestHits.computeIfAbsent(ip, ignored -> new AtomicInteger())
            .incrementAndGet() <= accountProperties.rateLimit();
    }

    public void requireCreate(String ip)
    {
        if (ip == null || ip.isBlank())
            return;
        if (creates.computeIfAbsent(ip, ignored -> new AtomicInteger())
            .incrementAndGet() > accountProperties.createQuota()
        )
        {
            throw new AccountException("no", HttpStatus.TOO_MANY_REQUESTS);
        }
    }

    void clearRequestHits() {
        requestHits.clear();
    }

    void clearCreates() {
        creates.clear();
    }
}
