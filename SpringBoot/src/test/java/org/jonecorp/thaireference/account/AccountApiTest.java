package org.jonecorp.thaireference.account;

import static org.assertj.core.api.Assertions.assertThat;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.post;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.header;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.redirectedUrlPattern;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import org.jonecorp.thaireference.account.mail.MailCodeRecorder;
import org.jonecorp.thaireference.account.repository.DeviceGroupRepository;
import org.jonecorp.thaireference.account.repository.DeviceRepository;
import java.util.UUID;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.web.servlet.AutoConfigureMockMvc;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.context.annotation.Import;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.test.context.ActiveProfiles;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.ResultActions;

@SpringBootTest
@AutoConfigureMockMvc
@ActiveProfiles("test")
@Import(MailCodeRecorder.class)
class AccountApiTest {

    @Autowired
    MockMvc mockMvc;

    @Autowired
    DeviceRepository deviceRepository;

    @Autowired
    DeviceGroupRepository groupRepository;

    @Autowired
    MailCodeRecorder mailCodeRecorder;

    @Test
    void firstCheckinCreatesSoloGroupAndSecondCallReusesIt() throws Exception {
        UUID uuid = UUID.randomUUID();
        String first = checkin(uuid)
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.created").value(true))
                .andExpect(jsonPath("$.device_uuid").value(uuid.toString()))
                .andExpect(jsonPath("$.email_set").value(false))
                .andExpect(jsonPath("$.devices.length()").value(1))
                .andExpect(jsonPath("$.devices[0].last_ip").value("127.0.0.1"))
                .andReturn()
                .getResponse()
                .getContentAsString();
        long groupId = groupIdFrom(first);
        assertThat(deviceRepository.findByUuid(uuid)).isPresent();
        assertThat(groupRepository.findById(groupId)).isPresent();
        assertThat(deviceRepository.findByUuid(uuid).orElseThrow().getLastIp()).isEqualTo("127.0.0.1");

        checkin(uuid)
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.created").value(false))
                .andExpect(jsonPath("$.group_id").value((int) groupId));
        assertThat(deviceRepository.findByUuid(uuid).orElseThrow().getGroupId()).isEqualTo(groupId);
        assertThat(groupRepository.count()).isGreaterThanOrEqualTo(1);

        info(uuid)
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.created").value(false))
                .andExpect(jsonPath("$.group_id").value((int) groupId))
                .andExpect(jsonPath("$.devices[0].last_ip").value("127.0.0.1"));
    }

    @Test
    void infoWithoutCheckinReturnsJson401() throws Exception {
        info(UUID.randomUUID())
                .andExpect(status().isUnauthorized())
                .andExpect(jsonPath("$.error").value("unauthorized"));
    }

    @Test
    void checkinStoresLastIpFromXForwardedFor() throws Exception {
        UUID uuid = UUID.randomUUID();
        mockMvc.perform(post("/account/checkin")
                        .header(HttpHeaders.AUTHORIZATION, uuid.toString())
                        .header("X-Forwarded-For", "203.0.113.10, 10.0.0.1")
                        .header("X-Platform", "test")
                        .header("X-Build-Number", "1"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.devices[0].this_device").value(true))
                .andExpect(jsonPath("$.devices[0].last_ip").value("203.0.113.10"));
        assertThat(deviceRepository.findByUuid(uuid).orElseThrow().getLastIp()).isEqualTo("203.0.113.10");

        mockMvc.perform(get("/account/info")
                        .header(HttpHeaders.AUTHORIZATION, uuid.toString())
                        .header("X-Forwarded-For", "198.51.100.1"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.devices[0].last_ip").value("203.0.113.10"));
        assertThat(deviceRepository.findByUuid(uuid).orElseThrow().getLastIp()).isEqualTo("203.0.113.10");
    }

    @Test
    void pairStartWithoutHeaderOrUnknownUuidReturnsJson401() throws Exception {
        mockMvc.perform(post("/account/devices/pair/start"))
                .andExpect(status().isUnauthorized())
                .andExpect(header().doesNotExist(HttpHeaders.LOCATION))
                .andExpect(jsonPath("$.error").value("unauthorized"));

        mockMvc.perform(post("/account/devices/pair/start")
                        .header(HttpHeaders.AUTHORIZATION, UUID.randomUUID().toString()))
                .andExpect(status().isUnauthorized())
                .andExpect(header().doesNotExist(HttpHeaders.LOCATION))
                .andExpect(jsonPath("$.error").value("unauthorized"));
    }

    @Test
    void adminWithoutSessionRedirectsToLogin() throws Exception {
        mockMvc.perform(get("/admin/resource"))
                .andExpect(status().is3xxRedirection())
                .andExpect(redirectedUrlPattern("**/login"));
    }

    @Test
    void ownershipControlsEmailAndTransferAndModelIsStored() throws Exception {
        UUID host = UUID.randomUUID();
        UUID other = UUID.randomUUID();
        mockMvc.perform(post("/account/checkin")
                        .header(HttpHeaders.AUTHORIZATION, host.toString())
                        .header("X-Device-Model", "Test model"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.owner").value(true))
                .andExpect(jsonPath("$.devices[0].model").value("Test model"))
                .andExpect(jsonPath("$.devices[0].owner").value(true));
        checkin(other).andExpect(status().isOk());
        String token = tokenField(mockMvc.perform(post("/account/devices/pair/start")
                        .header(HttpHeaders.AUTHORIZATION, host.toString()))
                .andExpect(status().isOk()).andReturn().getResponse().getContentAsString());
        mockMvc.perform(post("/account/devices/pair/complete")
                        .header(HttpHeaders.AUTHORIZATION, other.toString())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"token\":\"" + token + "\"}"))
                .andExpect(status().isOk()).andExpect(jsonPath("$.owner").value(false));
        mockMvc.perform(post("/account/email")
                        .header(HttpHeaders.AUTHORIZATION, other.toString())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"email\":\"test@example.com\"}"))
                .andExpect(status().isForbidden());
        mockMvc.perform(post("/account/devices/unlink")
                        .header(HttpHeaders.AUTHORIZATION, host.toString())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"device_uuid\":\"" + host + "\"}"))
                .andExpect(status().isConflict());
        mockMvc.perform(post("/account/devices/owner")
                        .header(HttpHeaders.AUTHORIZATION, host.toString())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"device_uuid\":\"" + other + "\"}"))
                .andExpect(status().isOk()).andExpect(jsonPath("$.owner").value(false));
        info(other).andExpect(status().isOk()).andExpect(jsonPath("$.owner").value(true));
        mockMvc.perform(post("/account/email")
                        .header(HttpHeaders.AUTHORIZATION, host.toString())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"email\":\"test@example.com\"}"))
                .andExpect(status().isForbidden());
    }

    @Test
    void qrPairMovesScannerAndDeletesEmptyGroup() throws Exception {
        UUID host = UUID.randomUUID();
        UUID scanner = UUID.randomUUID();
        long hostGroup = groupIdFrom(checkin(host).andExpect(status().isOk()).andReturn().getResponse().getContentAsString());
        long scannerGroup =
                groupIdFrom(checkin(scanner).andExpect(status().isOk()).andReturn().getResponse().getContentAsString());
        assertThat(scannerGroup).isNotEqualTo(hostGroup);

        String token = mockMvc.perform(post("/account/devices/pair/start").header(HttpHeaders.AUTHORIZATION, host.toString()))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.token").isString())
                .andReturn()
                .getResponse()
                .getContentAsString();
        String rawToken = tokenField(token);

        mockMvc.perform(post("/account/devices/pair/complete")
                        .header(HttpHeaders.AUTHORIZATION, scanner.toString())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"token\":\"" + rawToken + "\"}"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.group_id").value((int) hostGroup))
                .andExpect(jsonPath("$.devices.length()").value(2));

        info(scanner)
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.group_id").value((int) hostGroup))
                .andExpect(jsonPath("$.devices.length()").value(2))
                .andExpect(jsonPath("$.devices[0].last_ip").doesNotExist())
                .andExpect(jsonPath("$.devices[1].this_device").value(true))
                .andExpect(jsonPath("$.devices[1].last_ip").value("127.0.0.1"));
        assertThat(groupRepository.findById(scannerGroup)).isEmpty();
    }

    @Test
    void unlinkMovesTargetToSoloGroup() throws Exception {
        UUID host = UUID.randomUUID();
        UUID other = UUID.randomUUID();
        checkin(host).andExpect(status().isOk());
        checkin(other).andExpect(status().isOk());
        String token = tokenField(mockMvc.perform(post("/account/devices/pair/start")
                        .header(HttpHeaders.AUTHORIZATION, host.toString()))
                .andExpect(status().isOk())
                .andReturn()
                .getResponse()
                .getContentAsString());
        mockMvc.perform(post("/account/devices/pair/complete")
                        .header(HttpHeaders.AUTHORIZATION, other.toString())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"token\":\"" + token + "\"}"))
                .andExpect(status().isOk());

        mockMvc.perform(post("/account/devices/unlink")
                        .header(HttpHeaders.AUTHORIZATION, host.toString())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"device_uuid\":\"" + other + "\"}"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.devices.length()").value(1))
                .andExpect(jsonPath("$.devices[0].uuid").value(host.toString()));

        String otherInfo = info(other).andExpect(status().isOk()).andReturn().getResponse().getContentAsString();
        String hostInfo = info(host).andExpect(status().isOk()).andReturn().getResponse().getContentAsString();
        assertThat(groupIdFrom(otherInfo)).isNotEqualTo(groupIdFrom(hostInfo));
        assertThat(deviceRepository.findByUuid(other).orElseThrow().getGroupId())
                .isNotEqualTo(deviceRepository.findByUuid(host).orElseThrow().getGroupId());
    }

    @Test
    void emailConfirmWithCodeAttachesEmail() throws Exception {
        UUID uuid = UUID.randomUUID();
        checkin(uuid).andExpect(status().isOk());
        String email = "user-" + uuid + "@example.com";
        mockMvc.perform(post("/account/email")
                        .header(HttpHeaders.AUTHORIZATION, uuid.toString())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"email\":\"" + email + "\"}"))
                .andExpect(status().isOk());
        String code = mailCodeRecorder.lastCode();
        assertThat(code).isNotBlank();

        mockMvc.perform(post("/account/email/confirm")
                        .header(HttpHeaders.AUTHORIZATION, uuid.toString())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"code\":\"" + code + "\"}"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.ok").value(true));

        info(uuid)
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.email_set").value(true))
                .andExpect(jsonPath("$.email").value(email));
    }

    @Test
    void recoverMovesDeviceIntoEmailedGroup() throws Exception {
        UUID host = UUID.randomUUID();
        checkin(host).andExpect(status().isOk());
        String email = "recover-" + host + "@example.com";
        mockMvc.perform(post("/account/email")
                        .header(HttpHeaders.AUTHORIZATION, host.toString())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"email\":\"" + email + "\"}"))
                .andExpect(status().isOk());
        mockMvc.perform(post("/account/email/confirm")
                        .header(HttpHeaders.AUTHORIZATION, host.toString())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"code\":\"" + mailCodeRecorder.lastCode() + "\"}"))
                .andExpect(status().isOk());
        long hostGroup = groupIdFrom(info(host).andExpect(status().isOk()).andReturn().getResponse().getContentAsString());

        UUID lost = UUID.randomUUID();
        checkin(lost).andExpect(status().isOk());
        mockMvc.perform(post("/account/recover")
                        .header(HttpHeaders.AUTHORIZATION, lost.toString())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"email\":\"" + email + "\"}"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.ok").value(true));
        mockMvc.perform(post("/account/recover/confirm")
                        .header(HttpHeaders.AUTHORIZATION, lost.toString())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"code\":\"" + mailCodeRecorder.lastCode() + "\"}"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.ok").value(true));

        info(lost)
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.group_id").value((int) hostGroup))
                .andExpect(jsonPath("$.owner").value(true))
                .andExpect(jsonPath("$.email_set").value(true));
        info(host).andExpect(status().isOk()).andExpect(jsonPath("$.owner").value(false));
    }

    @Test
    void recoverUnknownEmailStillReturnsOk() throws Exception {
        UUID uuid = UUID.randomUUID();
        checkin(uuid).andExpect(status().isOk());
        mockMvc.perform(post("/account/recover")
                        .header(HttpHeaders.AUTHORIZATION, uuid.toString())
                        .contentType(MediaType.APPLICATION_JSON)
                        .content("{\"email\":\"missing-" + uuid + "@example.com\"}"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$.ok").value(true));
    }

    private ResultActions checkin(UUID uuid) throws Exception {
        return mockMvc.perform(post("/account/checkin")
                .header(HttpHeaders.AUTHORIZATION, uuid.toString())
                .header("X-Platform", "test")
                .header("X-Build-Number", "1"));
    }

    private ResultActions info(UUID uuid) throws Exception {
        return mockMvc.perform(get("/account/info").header(HttpHeaders.AUTHORIZATION, uuid.toString()));
    }

    private static long groupIdFrom(String json) {
        int start = json.indexOf("\"group_id\":") + "\"group_id\":".length();
        int end = start;
        while (end < json.length() && Character.isDigit(json.charAt(end))) {
            end++;
        }
        return Long.parseLong(json.substring(start, end));
    }

    private static String tokenField(String json) {
        int start = json.indexOf("\"token\":\"") + "\"token\":\"".length();
        int end = json.indexOf('"', start);
        return json.substring(start, end);
    }
}
