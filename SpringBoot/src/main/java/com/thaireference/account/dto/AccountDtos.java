package com.thaireference.account.dto;

import java.time.Instant;
import java.util.List;
import java.util.UUID;
import lombok.experimental.UtilityClass;

@UtilityClass
public class AccountDtos {

    public record AccountInfoResponse(
            boolean created,
            Long groupId,
            UUID deviceUuid,
            boolean emailSet,
            String email,
            List<DeviceMemberDto> devices) {}

    public record DeviceMemberDto(
            UUID uuid,
            String platform,
            String build,
            Instant linkedAt,
            Instant lastSeen,
            boolean thisDevice,
            String lastIp) {}

    public record PairStartResponse(String token, Instant expiresAt) {}

    public record PairCompleteRequest(String token) {}

    public record UnlinkRequest(UUID deviceUuid) {}

    public record EmailRequest(String email) {}

    public record CodeRequest(String code) {}

    public record OkResponse(boolean ok) {
        public static OkResponse OK = new OkResponse(true);
    }
}
