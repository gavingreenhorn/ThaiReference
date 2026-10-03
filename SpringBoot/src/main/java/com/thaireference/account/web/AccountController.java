package com.thaireference.account.web;

import com.thaireference.account.AccountService;
import com.thaireference.exception.AccountException;
import com.thaireference.util.OriginatingIp;
import com.thaireference.account.dto.AccountDtos.AccountInfoResponse;
import com.thaireference.account.dto.AccountDtos.CodeRequest;
import com.thaireference.account.dto.AccountDtos.EmailRequest;
import com.thaireference.account.dto.AccountDtos.OkResponse;
import com.thaireference.account.dto.AccountDtos.PairCompleteRequest;
import com.thaireference.account.dto.AccountDtos.PairStartResponse;
import com.thaireference.account.dto.AccountDtos.UnlinkRequest;
import com.thaireference.security.DevicePrincipal;
import jakarta.servlet.http.HttpServletRequest;
import java.util.UUID;
import org.springframework.http.HttpHeaders;
import org.springframework.security.core.annotation.AuthenticationPrincipal;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestHeader;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/account")
public class AccountController {

    private final AccountService accountService;

    public AccountController(AccountService accountService) {
        this.accountService = accountService;
    }

    @PostMapping("/checkin")
    public AccountInfoResponse checkin(
            @RequestHeader(value = HttpHeaders.AUTHORIZATION, required = false) String authorization,
            @RequestHeader(value = "X-Platform", required = false) String platform,
            @RequestHeader(value = "X-Build-Number", required = false) String build,
            HttpServletRequest request) {
        UUID uuid = AccountService.parseDeviceUuid(authorization);
        return accountService.checkin(uuid, platform, build, OriginatingIp.resolve(request));
    }

    @GetMapping("/info")
    public AccountInfoResponse info(@AuthenticationPrincipal DevicePrincipal device) {
        return accountService.getInfo(requireDevice(device));
    }

    @PostMapping("/devices/pair/start")
    public PairStartResponse startPair(@AuthenticationPrincipal DevicePrincipal device) {
        return accountService.startPair(requireDevice(device));
    }

    @PostMapping("/devices/pair/complete")
    public AccountInfoResponse completePair(
            @AuthenticationPrincipal DevicePrincipal device, @RequestBody PairCompleteRequest body) {
        String token = body == null ? null : body.token();
        return accountService.completePair(requireDevice(device), token);
    }

    @PostMapping("/devices/unlink")
    public AccountInfoResponse unlink(
            @AuthenticationPrincipal DevicePrincipal device, @RequestBody UnlinkRequest body) {
        UUID target = body == null ? null : body.deviceUuid();
        return accountService.unlink(requireDevice(device), target);
    }

    @PostMapping("/email")
    public OkResponse requestEmail(@AuthenticationPrincipal DevicePrincipal device, @RequestBody EmailRequest body) {
        accountService.requestAttachEmail(requireDevice(device), body == null ? null : body.email());
        return OkResponse.OK;
    }

    @PostMapping("/email/confirm")
    public OkResponse confirmEmail(
            @AuthenticationPrincipal DevicePrincipal device, @RequestBody CodeRequest body) {
        accountService.confirmEmail(requireDevice(device), body == null ? null : body.code());
        return OkResponse.OK;
    }

    @PostMapping("/recover")
    public OkResponse requestRecover(@AuthenticationPrincipal DevicePrincipal device, @RequestBody EmailRequest body) {
        accountService.requestRecover(requireDevice(device), body == null ? null : body.email());
        return OkResponse.OK;
    }

    @PostMapping("/recover/confirm")
    public OkResponse confirmRecover(
            @AuthenticationPrincipal DevicePrincipal device, @RequestBody CodeRequest body) {
        accountService.confirmRecover(requireDevice(device), body == null ? null : body.code());
        return OkResponse.OK;
    }

    private static DevicePrincipal requireDevice(DevicePrincipal device) {
        if (device == null) {
            throw new AccountException("unauthorized", org.springframework.http.HttpStatus.UNAUTHORIZED);
        }
        return device;
    }
}
