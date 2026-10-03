package com.thaireference.account;

import static org.assertj.core.api.Assertions.assertThat;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import com.thaireference.account.repository.DeviceRepository;
import java.util.UUID;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.http.HttpHeaders;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.test.context.TestPropertySource;
import org.springframework.test.web.servlet.MockMvc;

@SpringBootTest
@AutoConfigureMockMvc
@ActiveProfiles("test")
@TestPropertySource(properties = "app.account.create-quota=3")
class AccountApiCreateQuotaTest {

    @Autowired
    MockMvc mockMvc;

    @Autowired
    DeviceRepository deviceRepository;

    @Test
    void fourthNewDeviceFromSameIpIs429AndNotInserted() throws Exception {
        String ip = "203.0.113.80";
        UUID first = UUID.randomUUID();
        UUID second = UUID.randomUUID();
        UUID third = UUID.randomUUID();
        UUID fourth = UUID.randomUUID();
        checkin(first, ip).andExpect(status().isOk()).andExpect(jsonPath("$.created").value(true));
        checkin(second, ip).andExpect(status().isOk()).andExpect(jsonPath("$.created").value(true));
        checkin(third, ip).andExpect(status().isOk()).andExpect(jsonPath("$.created").value(true));
        checkin(first, ip).andExpect(status().isOk()).andExpect(jsonPath("$.created").value(false));
        checkin(fourth, ip)
                .andExpect(status().isTooManyRequests())
                .andExpect(jsonPath("$.error").value("no"));
        assertThat(deviceRepository.findByUuid(fourth)).isEmpty();
        checkin(UUID.randomUUID(), "198.51.100.80").andExpect(status().isOk()).andExpect(jsonPath("$.created").value(true));
    }

    private org.springframework.test.web.servlet.ResultActions checkin(UUID uuid, String ip) throws Exception {
        return mockMvc.perform(post("/account/checkin")
                .header(HttpHeaders.AUTHORIZATION, uuid.toString())
                .header("X-Forwarded-For", ip)
                .header("X-Platform", "test")
                .header("X-Build-Number", "1"));
    }
}
