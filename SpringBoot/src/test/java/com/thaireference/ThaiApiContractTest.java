package com.thaireference;

import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import com.thaireference.dto.HomonymsDto;
import com.thaireference.repository.ThaiReadRepository;
import com.thaireference.web.ThaiController;
import java.util.List;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

class ThaiApiContractTest {

    private MockMvc mockMvc;

    @BeforeEach
    void setUp() {
        ThaiReadRepository repository = mock(ThaiReadRepository.class);
        when(repository.findSounds()).thenReturn(List.of());
        when(repository.findHomonyms()).thenReturn(new HomonymsDto());
        mockMvc = MockMvcBuilders.standaloneSetup(new ThaiController(repository)).build();
    }

    @Test
    void soundsEndpointReturnsOk() throws Exception {
        mockMvc.perform(get("/thai/sounds")).andExpect(status().isOk());
    }

    @Test
    void homonymsEndpointReturnsOk() throws Exception {
        mockMvc.perform(get("/thai/homonyms")).andExpect(status().isOk());
    }
}
