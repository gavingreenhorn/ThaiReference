package org.jonecorp.thaireference;

import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.PropertyNamingStrategies;
import org.jonecorp.thaireference.dto.MoodDto;
import org.jonecorp.thaireference.dto.MoodSentenceDto;
import org.jonecorp.thaireference.dto.SentenceTokenDto;
import org.jonecorp.thaireference.dto.TranscriptionDto;
import org.jonecorp.thaireference.dto.WordDto;
import org.jonecorp.thaireference.repository.ThaiReadRepository;
import org.jonecorp.thaireference.web.ThaiController;
import java.util.List;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.http.converter.json.MappingJackson2HttpMessageConverter;
import org.springframework.test.web.servlet.MockMvc;
import org.springframework.test.web.servlet.setup.MockMvcBuilders;

class ThaiApiContractTest {

    private MockMvc mockMvc;
    private ThaiReadRepository repository;

    @BeforeEach
    void setUp() {
        repository = mock(ThaiReadRepository.class);
        when(repository.findSounds()).thenReturn(List.of());
        when(repository.findHomophones()).thenReturn(List.of());
        ObjectMapper mapper = new ObjectMapper().setPropertyNamingStrategy(PropertyNamingStrategies.SNAKE_CASE);
        mockMvc = MockMvcBuilders.standaloneSetup(new ThaiController(repository))
                .setMessageConverters(new MappingJackson2HttpMessageConverter(mapper))
                .build();
    }

    @Test
    void soundsEndpointReturnsOk() throws Exception {
        mockMvc.perform(get("/thai/sounds")).andExpect(status().isOk());
    }

    @Test
    void homophonesEndpointReturnsOk() throws Exception {
        mockMvc.perform(get("/thai/homophones")).andExpect(status().isOk());
    }

    @Test
    void moodsEndpointPreservesWordOrderAndFormatting() throws Exception {
        WordDto happy = new WordDto(1, "สุข", new TranscriptionDto("sùk", "sùk"), List.of("happy"));
        WordDto why = new WordDto(2, "ทำไม", new TranscriptionDto("tam-mai", "tʰām.māj"), List.of("why"));
        MoodSentenceDto first = new MoodSentenceDto(3, "Why?", List.of(
                new SentenceTokenDto(why, " "),
                new SentenceTokenDto(happy, "?")));
        MoodSentenceDto second = new MoodSentenceDto(4, "Happy.", List.of(
                new SentenceTokenDto(happy, ".")));
        when(repository.findMoods()).thenReturn(List.of(new MoodDto(5, happy, List.of(first, second))));

        mockMvc.perform(get("/thai/moods"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$[0].word.value").value("สุข"))
                .andExpect(jsonPath("$[0].sentences[0].id").value(3))
                .andExpect(jsonPath("$[0].sentences[0].words[0].word.value").value("ทำไม"))
                .andExpect(jsonPath("$[0].sentences[0].words[0].following_text").value(" "))
                .andExpect(jsonPath("$[0].sentences[0].words[1].following_text").value("?"))
                .andExpect(jsonPath("$[0].sentences[0].source_url").doesNotHaveJsonPath())
                .andExpect(jsonPath("$[0].sentences[0].source_author").doesNotHaveJsonPath())
                .andExpect(jsonPath("$[0].sentences[0].source_license").doesNotHaveJsonPath())
                .andExpect(jsonPath("$[0].sentences[1].id").value(4));
    }
}
