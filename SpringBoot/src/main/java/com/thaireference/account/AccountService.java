package com.thaireference.account;

import com.thaireference.account.dto.AccountDtos.AccountInfoResponse;
import com.thaireference.account.dto.AccountDtos.DeviceMemberDto;
import com.thaireference.account.dto.AccountDtos.PairStartResponse;
import com.thaireference.account.entity.AccountToken;
import com.thaireference.account.entity.AccountTokenPurpose;
import com.thaireference.account.entity.Device;
import com.thaireference.account.entity.DeviceGroup;
import com.thaireference.account.mail.AccountMailPort;
import com.thaireference.account.mail.MailCodeSink;
import com.thaireference.config.properties.AccountProperties;
import com.thaireference.account.repository.AccountTokenRepository;
import com.thaireference.account.repository.DeviceGroupRepository;
import com.thaireference.account.repository.DeviceRepository;
import com.thaireference.exception.AccountException;
import com.thaireference.security.AccountRateLimits;
import com.thaireference.security.DevicePrincipal;
import com.thaireference.util.TokenHashes;
import java.time.Instant;
import java.util.List;
import java.util.Locale;
import java.util.Optional;
import java.util.UUID;
import org.springframework.beans.factory.ObjectProvider;
import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

@Service
public class AccountService {

    private static final int UUID_VERSION_4 = 4;

    private final DeviceGroupRepository groupRepository;
    private final DeviceRepository deviceRepository;
    private final AccountTokenRepository tokenRepository;
    private final AccountProperties accountProperties;
    private final AccountMailPort mail;
    private final ObjectProvider<MailCodeSink> mailCodeSink;
    private final AccountRateLimits rateLimits;

    public AccountService(
            DeviceGroupRepository groupRepository,
            DeviceRepository deviceRepository,
            AccountTokenRepository tokenRepository,
            AccountProperties accountProperties,
            AccountMailPort mail,
            ObjectProvider<MailCodeSink> mailCodeSink,
            AccountRateLimits rateLimits) {
        this.groupRepository = groupRepository;
        this.deviceRepository = deviceRepository;
        this.tokenRepository = tokenRepository;
        this.accountProperties = accountProperties;
        this.mail = mail;
        this.mailCodeSink = mailCodeSink;
        this.rateLimits = rateLimits;
    }

    @Transactional
    public AccountInfoResponse checkin(UUID uuid, String platform, String build, String model, String lastIp) {
        requireUuidV4(uuid);
        Optional<Device> existing = deviceRepository.findByUuid(uuid);
        boolean created = existing.isEmpty();
        Device device;
        Instant now = Instant.now();
        if (created) {
            rateLimits.requireCreate(lastIp);
            DeviceGroup group = new DeviceGroup();
            group.setMaxDevices(accountProperties.maxDevices());
            group.setCreatedAt(now);
            group = groupRepository.save(group);
            device = new Device();
            device.setUuid(uuid);
            device.setGroupId(group.getId());
            device.setPlatform(blankToNull(platform));
            device.setBuild(blankToNull(build));
            device.setModel(normalizeModel(model));
            device.setLastIp(lastIp);
            device.setCreatedAt(now);
            device.setLinkedAt(now);
            device.setLastSeen(now);
            device = deviceRepository.save(device);
            group.setOwnerDeviceId(device.getId());
            groupRepository.save(group);
        } else {
            device = existing.get();
            device.setLastSeen(now);
            if (platform != null && !platform.isBlank()) {
                device.setPlatform(platform.trim());
            }
            if (build != null && !build.isBlank()) {
                device.setBuild(build.trim());
            }
            if (model != null && !model.isBlank()) {
                device.setModel(normalizeModel(model));
            }
            if (lastIp != null) {
                device.setLastIp(lastIp);
            }
            device = deviceRepository.save(device);
        }
        return toInfo(device, created);
    }

    @Transactional(readOnly = true)
    public AccountInfoResponse getInfo(DevicePrincipal principal) {
        Device device = deviceRepository
                .findById(principal.deviceId())
                .orElseThrow(() -> new AccountException("unauthorized", HttpStatus.UNAUTHORIZED));
        return toInfo(device, false);
    }

    @Transactional
    public PairStartResponse startPair(DevicePrincipal principal) {
        Instant now = Instant.now();
        String raw = TokenHashes.newRawToken();
        AccountToken token = new AccountToken();
        token.setTokenHash(TokenHashes.sha256(raw));
        token.setPurpose(AccountTokenPurpose.pair);
        token.setGroupId(principal.groupId());
        token.setDeviceUuid(principal.uuid());
        token.setExpiresAtEpochMs(now.plus(accountProperties.pairTtl()).toEpochMilli());
        token.setCreatedAt(now);
        tokenRepository.saveAndFlush(token);
        return new PairStartResponse(raw, Instant.ofEpochMilli(token.getExpiresAtEpochMs()));
    }

    @Transactional
    public AccountInfoResponse completePair(DevicePrincipal scanner, String rawToken) {
        AccountToken token = requireFresh(rawToken, AccountTokenPurpose.pair);
        Device device = deviceRepository
                .findByUuid(scanner.uuid())
                .orElseThrow(() -> new AccountException("unauthorized", HttpStatus.UNAUTHORIZED));
        DeviceGroup target = groupRepository
                .findById(token.getGroupId())
                .orElseThrow(() -> new AccountException("invalid_token", HttpStatus.BAD_REQUEST));
        if (!device.getGroupId().equals(target.getId())) {
            DeviceGroup current = requireGroup(device.getGroupId());
            if (device.getId().equals(current.getOwnerDeviceId())
                    && deviceRepository.countByGroupId(current.getId()) > 1) {
                throw new AccountException("transfer_owner_first", HttpStatus.CONFLICT);
            }
            if (deviceRepository.countByGroupId(target.getId()) >= target.getMaxDevices()) {
                throw new AccountException("max_devices", HttpStatus.CONFLICT);
            }
            Long oldGroupId = device.getGroupId();
            device.setGroupId(target.getId());
            device.setLinkedAt(Instant.now());
            deviceRepository.save(device);
            deleteGroupIfEmpty(oldGroupId);
        }
        token.setUsedAt(Instant.now());
        tokenRepository.save(token);
        return toInfo(device, false);
    }

    @Transactional
    public AccountInfoResponse unlink(DevicePrincipal actor, UUID targetUuid) {
        if (targetUuid == null) {
            throw new AccountException("not_found", HttpStatus.NOT_FOUND);
        }
        Device target = deviceRepository
                .findByUuid(targetUuid)
                .orElseThrow(() -> new AccountException("not_found", HttpStatus.NOT_FOUND));
        if (!target.getGroupId().equals(actor.groupId())) {
            throw new AccountException("not_found", HttpStatus.NOT_FOUND);
        }
        DeviceGroup current = requireGroup(actor.groupId());
        if (target.getId().equals(current.getOwnerDeviceId())
                && deviceRepository.countByGroupId(current.getId()) > 1) {
            throw new AccountException("transfer_owner_first", HttpStatus.CONFLICT);
        }
        Instant now = Instant.now();
        Long oldGroupId = target.getGroupId();
        DeviceGroup solo = new DeviceGroup();
        solo.setMaxDevices(accountProperties.maxDevices());
        solo.setCreatedAt(now);
        solo = groupRepository.save(solo);
        solo.setOwnerDeviceId(target.getId());
        groupRepository.save(solo);
        target.setGroupId(solo.getId());
        target.setLinkedAt(now);
        deviceRepository.save(target);
        deleteGroupIfEmpty(oldGroupId);
        Device actorDevice = deviceRepository
                .findByUuid(actor.uuid())
                .orElseThrow(() -> new AccountException("unauthorized", HttpStatus.UNAUTHORIZED));
        return toInfo(actorDevice, false);
    }

    @Transactional
    public void requestAttachEmail(DevicePrincipal principal, String email) {
        String normalized = normalizeEmail(email);
        DeviceGroup group = groupRepository
                .findById(principal.groupId())
                .orElseThrow(() -> new AccountException("unauthorized", HttpStatus.UNAUTHORIZED));
        requireOwner(group, principal);
        if (normalized.equals(group.getEmail())) {
            return;
        }
        groupRepository.findByEmail(normalized).ifPresent(other -> {
            if (!other.getId().equals(group.getId())) {
                throw new AccountException("email_in_use", HttpStatus.CONFLICT);
            }
        });
        Instant now = Instant.now();
        String code = TokenHashes.newSixDigitCode();
        AccountToken token = new AccountToken();
        token.setTokenHash(TokenHashes.sha256(code));
        token.setPurpose(AccountTokenPurpose.attach_email);
        token.setGroupId(group.getId());
        token.setDeviceUuid(principal.uuid());
        token.setEmail(normalized);
        token.setExpiresAtEpochMs(now.plus(accountProperties.emailTtl()).toEpochMilli());
        token.setCreatedAt(now);
        tokenRepository.saveAndFlush(token);
        mailCodeSink.ifAvailable(sink -> sink.record(code));
        mail.send(normalized, "Confirm your email", "Your code is: " + code);
    }

    @Transactional
    public void confirmEmail(DevicePrincipal principal, String code) {
        AccountToken token = requireFresh(code, AccountTokenPurpose.attach_email);
        if (!principal.uuid().equals(token.getDeviceUuid())) {
            throw new AccountException("unauthorized", HttpStatus.UNAUTHORIZED);
        }
        String email = token.getEmail();
        if (email == null || email.isBlank()) {
            throw new AccountException("invalid_token", HttpStatus.BAD_REQUEST);
        }
        groupRepository.findByEmail(email).ifPresent(other -> {
            if (!other.getId().equals(token.getGroupId())) {
                throw new AccountException("email_in_use", HttpStatus.CONFLICT);
            }
        });
        DeviceGroup group = groupRepository
                .findById(token.getGroupId())
                .orElseThrow(() -> new AccountException("invalid_token", HttpStatus.BAD_REQUEST));
        requireOwner(group, principal);
        group.setEmail(email);
        groupRepository.save(group);
        token.setUsedAt(Instant.now());
        tokenRepository.save(token);
    }

    @Transactional
    public void requestRecover(DevicePrincipal principal, String email) {
        String normalized = normalizeEmail(email);
        groupRepository.findByEmail(normalized).ifPresent(group -> {
            Instant now = Instant.now();
            String code = TokenHashes.newSixDigitCode();
            AccountToken token = new AccountToken();
            token.setTokenHash(TokenHashes.sha256(code));
            token.setPurpose(AccountTokenPurpose.recover);
            token.setGroupId(group.getId());
            token.setDeviceUuid(principal.uuid());
            token.setEmail(normalized);
            token.setExpiresAtEpochMs(now.plus(accountProperties.emailTtl()).toEpochMilli());
            token.setCreatedAt(now);
            tokenRepository.saveAndFlush(token);
            mailCodeSink.ifAvailable(sink -> sink.record(code));
            mail.send(normalized, "Recover your devices", "Your code is: " + code);
        });
    }

    @Transactional
    public void confirmRecover(DevicePrincipal principal, String code) {
        AccountToken token = requireFresh(code, AccountTokenPurpose.recover);
        if (!principal.uuid().equals(token.getDeviceUuid())) {
            throw new AccountException("unauthorized", HttpStatus.UNAUTHORIZED);
        }
        Device device = deviceRepository
                .findByUuid(principal.uuid())
                .orElseThrow(() -> new AccountException("unauthorized", HttpStatus.UNAUTHORIZED));
        DeviceGroup target = groupRepository
                .findById(token.getGroupId())
                .orElseThrow(() -> new AccountException("invalid_token", HttpStatus.BAD_REQUEST));
        if (!device.getGroupId().equals(target.getId())) {
            DeviceGroup current = requireGroup(device.getGroupId());
            if (device.getId().equals(current.getOwnerDeviceId())
                    && deviceRepository.countByGroupId(current.getId()) > 1) {
                throw new AccountException("transfer_owner_first", HttpStatus.CONFLICT);
            }
            if (deviceRepository.countByGroupId(target.getId()) >= target.getMaxDevices()) {
                throw new AccountException("max_devices", HttpStatus.CONFLICT);
            }
            Long oldGroupId = device.getGroupId();
            device.setGroupId(target.getId());
            device.setLinkedAt(Instant.now());
            deviceRepository.save(device);
            deleteGroupIfEmpty(oldGroupId);
        }
        target.setOwnerDeviceId(device.getId());
        groupRepository.save(target);
        token.setUsedAt(Instant.now());
        tokenRepository.save(token);
    }

    public static UUID parseDeviceUuid(String authorization) {
        if (authorization == null || authorization.isBlank()) {
            throw new AccountException("invalid_uuid", HttpStatus.BAD_REQUEST);
        }
        try {
            UUID uuid = UUID.fromString(authorization.trim());
            requireUuidV4(uuid);
            return uuid;
        } catch (IllegalArgumentException ex) {
            throw new AccountException("invalid_uuid", HttpStatus.BAD_REQUEST);
        }
    }

    private AccountInfoResponse toInfo(Device device, boolean created) {
        DeviceGroup group = groupRepository
                .findById(device.getGroupId())
                .orElseThrow(() -> new AccountException("unauthorized", HttpStatus.UNAUTHORIZED));
        List<DeviceMemberDto> members = deviceRepository.findByGroupIdOrderByLinkedAtAscIdAsc(group.getId()).stream()
                .map(member -> {
                    boolean thisDevice = member.getUuid().equals(device.getUuid());
                    return new DeviceMemberDto(
                            member.getUuid(),
                            member.getModel(),
                            member.getPlatform(),
                            member.getBuild(),
                            member.getLinkedAt(),
                            member.getLastSeen(),
                            thisDevice,
                            member.getId().equals(group.getOwnerDeviceId()),
                            thisDevice ? member.getLastIp() : null);
                })
                .toList();
        boolean emailSet = group.getEmail() != null && !group.getEmail().isBlank();
        return new AccountInfoResponse(
                created, group.getId(), device.getUuid(), emailSet, emailSet ? group.getEmail() : null,
                device.getId().equals(group.getOwnerDeviceId()), members);
    }

    @Transactional
    public AccountInfoResponse transferOwner(DevicePrincipal principal, UUID targetUuid) {
        DeviceGroup group = requireGroup(principal.groupId());
        requireOwner(group, principal);
        Device target = targetUuid == null ? null : deviceRepository.findByUuid(targetUuid).orElse(null);
        if (target == null || !target.getGroupId().equals(group.getId())) {
            throw new AccountException("not_found", HttpStatus.NOT_FOUND);
        }
        group.setOwnerDeviceId(target.getId());
        groupRepository.save(group);
        Device actor = deviceRepository.findById(principal.deviceId())
                .orElseThrow(() -> new AccountException("unauthorized", HttpStatus.UNAUTHORIZED));
        return toInfo(actor, false);
    }

    private DeviceGroup requireGroup(Long groupId) {
        return groupRepository.findById(groupId)
                .orElseThrow(() -> new AccountException("unauthorized", HttpStatus.UNAUTHORIZED));
    }

    private static void requireOwner(DeviceGroup group, DevicePrincipal principal) {
        if (!principal.deviceId().equals(group.getOwnerDeviceId())) {
            throw new AccountException("owner_required", HttpStatus.FORBIDDEN);
        }
    }

    private static String normalizeModel(String model) {
        String value = blankToNull(model);
        if (value != null && value.length() > 128) {
            throw new AccountException("invalid_model", HttpStatus.BAD_REQUEST);
        }
        return value;
    }

    private void deleteGroupIfEmpty(Long groupId) {
        if (deviceRepository.countByGroupId(groupId) > 0) {
            return;
        }
        tokenRepository.deleteByGroupId(groupId);
        groupRepository.deleteById(groupId);
    }

    private AccountToken requireFresh(String rawToken, AccountTokenPurpose purpose) {
        if (rawToken == null || rawToken.isBlank()) {
            throw new AccountException("invalid_token", HttpStatus.BAD_REQUEST);
        }
        AccountToken token = tokenRepository
                .findFresh(TokenHashes.sha256(rawToken.trim()), purpose)
                .orElseThrow(() -> new AccountException("invalid_token", HttpStatus.BAD_REQUEST));
        if (Instant.now().toEpochMilli() >= token.getExpiresAtEpochMs()) {
            throw new AccountException("invalid_token", HttpStatus.BAD_REQUEST);
        }
        return token;
    }

    private static void requireUuidV4(UUID uuid) {
        if (uuid.version() != UUID_VERSION_4) {
            throw new AccountException("invalid_uuid", HttpStatus.BAD_REQUEST);
        }
    }

    private static String normalizeEmail(String email) {
        String normalized = email == null ? "" : email.trim().toLowerCase(Locale.ROOT);
        int at = normalized.indexOf('@');
        if (normalized.isEmpty() || at < 1 || at == normalized.length() - 1) {
            throw new AccountException("invalid_email", HttpStatus.BAD_REQUEST);
        }
        return normalized;
    }

    private static String blankToNull(String value) {
        if (value == null || value.isBlank()) {
            return null;
        }
        return value.trim();
    }
}
