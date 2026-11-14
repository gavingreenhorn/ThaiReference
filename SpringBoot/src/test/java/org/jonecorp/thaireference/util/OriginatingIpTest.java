package org.jonecorp.thaireference.util;

import static org.assertj.core.api.Assertions.assertThat;

import org.junit.jupiter.api.Test;
import org.springframework.mock.web.MockHttpServletRequest;

class OriginatingIpTest {

    @Test
    void prefersFirstXForwardedForHop() {
        MockHttpServletRequest request = new MockHttpServletRequest();
        request.addHeader("X-Forwarded-For", "203.0.113.10, 10.0.0.1");
        request.addHeader("X-Real-IP", "198.51.100.1");
        request.setRemoteAddr("127.0.0.1");
        assertThat(OriginatingIp.resolve(request)).isEqualTo("203.0.113.10");
    }

    @Test
    void usesXRealIpWhenForwardedForMissing() {
        MockHttpServletRequest request = new MockHttpServletRequest();
        request.addHeader("X-Real-IP", "198.51.100.1");
        request.setRemoteAddr("127.0.0.1");
        assertThat(OriginatingIp.resolve(request)).isEqualTo("198.51.100.1");
    }

    @Test
    void fallsBackToRemoteAddr() {
        MockHttpServletRequest request = new MockHttpServletRequest();
        request.setRemoteAddr("127.0.0.1");
        assertThat(OriginatingIp.resolve(request)).isEqualTo("127.0.0.1");
    }

    @Test
    void acceptsIpv6() {
        MockHttpServletRequest request = new MockHttpServletRequest();
        request.setRemoteAddr("2001:db8::1");
        assertThat(OriginatingIp.resolve(request)).isEqualTo("2001:db8:0:0:0:0:0:1");
    }

    @Test
    void rejectsInvalidLiterals() {
        assertThat(OriginatingIp.validate("localhost")).isNull();
        assertThat(OriginatingIp.validate("not-an-ip")).isNull();
        assertThat(OriginatingIp.validate("999.0.0.1")).isNull();
        MockHttpServletRequest request = new MockHttpServletRequest();
        request.addHeader("X-Forwarded-For", "not-an-ip");
        request.setRemoteAddr("127.0.0.1");
        assertThat(OriginatingIp.resolve(request)).isNull();
    }
}
