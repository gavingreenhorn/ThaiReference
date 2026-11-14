package org.jonecorp.thaireference.account.dto;

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
            boolean owner,
            List<DeviceMemberDto> devices) {}

    public record DeviceMemberDto(
            UUID uuid,
            String model,
            String platform,
            String build,
            Instant linkedAt,
            Instant lastSeen,
            boolean thisDevice,
            boolean owner,
            String lastIp) {}

    public record PairStartResponse(String token, Instant expiresAt) {}

    public record PairCompleteRequest(String token) {}

    public record UnlinkRequest(UUID deviceUuid) {}

    public record TransferOwnerRequest(UUID deviceUuid) {}

    public record EmailRequest(String email) {}

    public record CodeRequest(String code) {}

    public record OkResponse(boolean ok) {
        public static OkResponse OK = new OkResponse(true);
    }
}
