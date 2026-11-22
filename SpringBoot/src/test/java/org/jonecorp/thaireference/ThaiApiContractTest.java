package org.jonecorp.thaireference;

import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;
import static org.springframework.test.web.servlet.request.MockMvcRequestBuilders.get;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.header;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.jsonPath;
import static org.springframework.test.web.servlet.result.MockMvcResultMatchers.status;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.annotation.JsonInclude;
import com.fasterxml.jackson.databind.PropertyNamingStrategies;
import org.jonecorp.thaireference.dto.MoodDto;
import org.jonecorp.thaireference.dto.ColorLabel;
import org.jonecorp.thaireference.dto.DirectionDto;
import org.jonecorp.thaireference.dto.HueFamilyDto;
import org.jonecorp.thaireference.dto.MarkDto;
import org.jonecorp.thaireference.dto.PositionDto;
import org.jonecorp.thaireference.dto.SentenceDto;
import org.jonecorp.thaireference.dto.SeasonDto;
import org.jonecorp.thaireference.dto.ThaiClassDto;
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
        ObjectMapper mapper = new ObjectMapper().setPropertyNamingStrategy(PropertyNamingStrategies.SNAKE_CASE)
                .setDefaultPropertyInclusion(JsonInclude.Include.NON_NULL);
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
    void namedDomainsExposeWholeWordsAndKeepTheNamelessToneMark() throws Exception {
        WordDto word = new WordDto(900, "ไม้เอก", new TranscriptionDto("mai-ek", "mai.ek"),
                List.of("first Thai tone mark"), List.of(45, 46));
        when(repository.findDirections()).thenReturn(List.of(new DirectionDto(1, "N", "north", word)));
        when(repository.findClasses()).thenReturn(List.of(new ThaiClassDto("low", "Low", word)));
        when(repository.findSeasons()).thenReturn(List.of(new SeasonDto(1, word, "Winter", "winter", List.of())));
        when(repository.findMarks()).thenReturn(List.of(
                new MarkDto("none", true, null, "No tone mark"),
                new MarkDto("mark", true, word, "Low tone"),
                new MarkDto("symbol", false, word)));

        for (String endpoint : List.of("directions", "classes", "seasons")) {
            mockMvc.perform(get("/thai/" + endpoint))
                    .andExpect(status().isOk())
                    .andExpect(jsonPath("$[0].word.id").value(900))
                    .andExpect(jsonPath("$[0].word.lexemes[0]").value("first Thai tone mark"))
                    .andExpect(jsonPath("$[0].word.component_word_ids[1]").value(46))
                    .andExpect(jsonPath("$[0].words").doesNotHaveJsonPath());
        }
        mockMvc.perform(get("/thai/marks"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$[0].word").hasJsonPath())
                .andExpect(jsonPath("$[0].word").isEmpty())
                .andExpect(jsonPath("$[1].word.id").value(900))
                .andExpect(jsonPath("$[2].word.id").value(900));
    }

    @Test
    void positionsExposeASingularWholeWordAndSeparatePhraseWords() throws Exception {
        WordDto whole = new WordDto(900, "whole", new TranscriptionDto("a-b", "a.b"),
                List.of("whole meaning", "other whole meaning"), List.of(1, 2));
        WordDto side = new WordDto(535, "side", new TranscriptionDto("c", "c"),
                List.of(), List.of());
        SentenceDto example = new SentenceDto(1, "example", List.of(side, whole));
        when(repository.findPositions()).thenReturn(List.of(
                new PositionDto(1, "domain meaning", whole, null, example),
                new PositionDto(13, "phrase meaning", null, List.of(side, whole), example)));

        mockMvc.perform(get("/thai/positions"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$[0].word.id").value(900))
                .andExpect(jsonPath("$[0].word.lexemes[1]").value("other whole meaning"))
                .andExpect(jsonPath("$[0].word.component_word_ids[1]").value(2))
                .andExpect(jsonPath("$[0].phrase_words").doesNotHaveJsonPath())
                .andExpect(jsonPath("$[0].words").doesNotHaveJsonPath())
                .andExpect(jsonPath("$[1].word").hasJsonPath())
                .andExpect(jsonPath("$[1].word").isEmpty())
                .andExpect(jsonPath("$[1].phrase_words[0].id").value(535))
                .andExpect(jsonPath("$[1].phrase_words[0].component_word_ids").doesNotHaveJsonPath())
                .andExpect(jsonPath("$[1].phrase_words[0].lexemes").isArray())
                .andExpect(jsonPath("$[1].phrase_words[0].lexemes").isEmpty())
                .andExpect(jsonPath("$[1].phrase_words[1].id").value(900))
                .andExpect(jsonPath("$[1].words").doesNotHaveJsonPath())
                .andExpect(jsonPath("$[0].example.words[1].id").value(900));
    }

    @Test
    void colorsRetainTheirLabelShapeForBothWholeWordsAndPhrases() throws Exception {
        WordDto whole = new WordDto(900, "maroon", new TranscriptionDto("a-b", "a.b"),
                List.of("maroon"), List.of(1, 2));
        WordDto modifier = new WordDto(901, "light", new TranscriptionDto("c", "c"), List.of("light"), List.of());
        when(repository.findColors()).thenReturn(List.of(new HueFamilyDto(1, "red", "Red", "#ff0000", 1,
                List.of(new ColorLabel(1, "#800000", "Maroon", List.of(whole), 1),
                        new ColorLabel(2, "#ffaaaa", "Light maroon", List.of(whole, modifier), 2)))));

        mockMvc.perform(get("/thai/colors"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$[0].colors[0].words[0].id").value(900))
                .andExpect(jsonPath("$[0].colors[0].words[0].lexemes[0]").value("maroon"))
                .andExpect(jsonPath("$[0].colors[0].words[0].component_word_ids[1]").value(2))
                .andExpect(jsonPath("$[0].colors[1].words[1].id").value(901));
    }

    @Test
    void moodsEndpointPreservesWordOrderAndFormatting() throws Exception {
        WordDto happy = new WordDto(1, "สุข", new TranscriptionDto("sùk", "sùk"), List.of("happy"), List.of());
        WordDto why = new WordDto(2, "ทำไม", new TranscriptionDto("tam-mai", "tʰām.māj"),
                List.of("why"), List.of(10, 11));
        MoodSentenceDto first = new MoodSentenceDto(3, "Why?", List.of(
                new SentenceTokenDto(why, " "),
                new SentenceTokenDto(happy, "?")));
        MoodSentenceDto second = new MoodSentenceDto(4, "Happy.", List.of(
                new SentenceTokenDto(happy, ".")));
        String svg = "<svg xmlns=\"http://www.w3.org/2000/svg\"/>";
        when(repository.findMoods()).thenReturn(List.of(new MoodDto(5, happy, svg, List.of(first, second))));

        mockMvc.perform(get("/thai/moods"))
                .andExpect(status().isOk())
                .andExpect(jsonPath("$[0].word.value").value("สุข"))
                .andExpect(jsonPath("$[0].svg").value(svg))
                .andExpect(jsonPath("$[0].sentences[0].id").value(3))
                .andExpect(jsonPath("$[0].sentences[0].words[0].word.value").value("ทำไม"))
                .andExpect(jsonPath("$[0].sentences[0].words[0].word.component_word_ids[0]").value(10))
                .andExpect(jsonPath("$[0].sentences[0].words[0].word.component_word_ids[1]").value(11))
                .andExpect(jsonPath("$[0].sentences[0].words[0].following_text").value(" "))
                .andExpect(jsonPath("$[0].sentences[0].words[1].following_text").value("?"))
                .andExpect(jsonPath("$[0].sentences[0].source_url").doesNotHaveJsonPath())
                .andExpect(jsonPath("$[0].sentences[0].source_author").doesNotHaveJsonPath())
                .andExpect(jsonPath("$[0].sentences[0].source_license").doesNotHaveJsonPath())
                .andExpect(jsonPath("$[0].sentences[1].id").value(4));
    }
}
