package com.thaireference.config;

import com.thaireference.config.properties.ReferenceDataProperties;
import com.thaireference.web.ThaiController;
import java.time.Instant;
import java.util.List;
import java.util.concurrent.ScheduledFuture;
import java.util.concurrent.locks.ReentrantLock;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.boot.context.event.ApplicationReadyEvent;
import org.springframework.cache.CacheManager;
import org.springframework.context.event.EventListener;
import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.scheduling.TaskScheduler;
import org.springframework.stereotype.Service;


@Service
public class ReferenceDataRefreshService {

    static final List<String> REFERENCE_CACHE_NAMES = List.of(
            "thai.sounds",
            "thai.consonants",
            "thai.clusters",
            "thai.homophones",
            "thai.minimalSets",
            "thai.vowels",
            "thai.marks",
            "thai.classes",
            "thai.numerals",
            "thai.pronouns",
            "thai.gridPositions",
            "thai.positions",
            "thai.weekdays",
            "thai.celestialBodies",
            "thai.timeOfDay",
            "thai.hours",
            "thai.months",
            "thai.directions",
            "thai.colors");

    private static final Logger log = LoggerFactory.getLogger(ReferenceDataRefreshService.class);

    private final JdbcTemplate jdbcTemplate;
    private final CacheManager cacheManager;
    private final ThaiController thaiController;
    private final ReferenceDataProperties properties;
    private final TaskScheduler taskScheduler;
    private final ReentrantLock refreshLock = new ReentrantLock();
    private volatile ScheduledFuture<?> pendingAdminRefresh;

    public ReferenceDataRefreshService(
            JdbcTemplate jdbcTemplate,
            CacheManager cacheManager,
            ThaiController thaiController,
            ReferenceDataProperties properties,
            TaskScheduler taskScheduler) {
        this.jdbcTemplate = jdbcTemplate;
        this.cacheManager = cacheManager;
        this.thaiController = thaiController;
        this.properties = properties;
        this.taskScheduler = taskScheduler;
    }

    @EventListener(ApplicationReadyEvent.class)
    public void refreshOnStartup() {
        refreshAll("startup");
        taskScheduler.scheduleWithFixedDelay(
                () -> refreshAll("scheduled"),
                Instant.now().plus(properties.refreshInterval()),
                properties.refreshInterval());
    }

    public void scheduleRefreshAfterAdminWrite() {
        ScheduledFuture<?> pending = pendingAdminRefresh;
        if (pending != null) {
            pending.cancel(false);
        }
        pendingAdminRefresh = taskScheduler.schedule(
                () -> refreshAll("admin-write"), Instant.now().plus(properties.adminDebounce()));
    }

    public void refreshAll(String trigger) {
        if (!refreshLock.tryLock()) {
            log.debug("Reference-data refresh already running (trigger={})", trigger);
            return;
        }
        long started = System.nanoTime();
        try {
            refreshWordView();
            refreshReferenceCaches();
            long ms = (System.nanoTime() - started) / 1_000_000;
            log.info("Reference-data refresh complete in {} ms (trigger={})", ms, trigger);
        } catch (RuntimeException ex) {
            log.warn("Reference-data refresh failed (trigger={}): {}", trigger, ex.getMessage());
        } finally {
            refreshLock.unlock();
        }
    }

    private void refreshWordView() {
        try {
            jdbcTemplate.execute("REFRESH MATERIALIZED VIEW CONCURRENTLY thai.\"WordView\"");
            log.debug("Refreshed materialized view thai.\"WordView\"");
        } catch (RuntimeException ex) {
            log.warn("Could not refresh thai.\"WordView\": {}", ex.getMessage());
        }
    }

    private void refreshReferenceCaches() {
        for (String name : REFERENCE_CACHE_NAMES) {
            var cache = cacheManager.getCache(name);
            if (cache != null) {
                cache.clear();
            }
        }
        thaiController.getSounds();
        thaiController.getConsonants();
        thaiController.getClusters();
        thaiController.getHomophones();
        thaiController.getMinimalSets();
        thaiController.getVowels();
        thaiController.getMarks();
        thaiController.getClasses();
        thaiController.getNumerals();
        thaiController.getPronouns();
        thaiController.getGridPositions();
        thaiController.getPositions();
        thaiController.getWeekdays();
        thaiController.getCelestialBodies();
        thaiController.getTimeOfDay();
        thaiController.getHours();
        thaiController.getMonths();
        thaiController.getDirections();
        thaiController.getColors();
    }
}
