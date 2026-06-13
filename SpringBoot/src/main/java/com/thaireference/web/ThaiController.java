package com.thaireference.web;

import com.thaireference.dto.*;
import com.thaireference.repository.ThaiReadRepository;
import java.util.List;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
@RequestMapping("/thai")
public class ThaiController {

    private final ThaiReadRepository repository;

    public ThaiController(ThaiReadRepository repository) {
        this.repository = repository;
    }

    @GetMapping("/sounds")
    public List<SoundDto> getSounds() {
        return repository.findSounds();
    }

    @GetMapping("/consonants")
    public List<ConsonantDto> getConsonants() {
        return repository.findConsonants();
    }

    @GetMapping("/clusters")
    public List<ClusterDto> getClusters() {
        return repository.findClusters();
    }

    @GetMapping("/homophones")
    public List<HomophoneGroupDto> getHomophones() {
        return repository.findHomophones();
    }

    @GetMapping("/vowels")
    public List<VowelDto> getVowels() {
        return repository.findVowels();
    }

    @GetMapping("/marks")
    public List<MarkDto> getMarks() {
        return repository.findMarks();
    }

    @GetMapping("/numerals")
    public List<NumeralDto> getNumerals() {
        return repository.findNumerals();
    }

    @GetMapping("/pronouns")
    public List<PronounDto> getPronouns() {
        return repository.findPronouns();
    }

    @GetMapping("/positions")
    public List<PositionCellDto> getPositions() {
        return repository.findPositions();
    }

    @GetMapping("/weekdays")
    public List<DayOfWeekDto> getWeekdays() {
        return repository.findWeekdays();
    }

    @GetMapping("/celestial-bodies")
    public List<CelestialBodyDto> getCelestialBodies() {
        return repository.findCelestialBodies();
    }

    @GetMapping("/time-of-day")
    public List<TimeOfDayDto> getTimeOfDay() {
        return repository.findTimeOfDay();
    }

    @GetMapping("/hours")
    public List<HourDto> getHours() {
        return repository.findHours();
    }

    @GetMapping("/months")
    public List<MonthDto> getMonths() {
        return repository.findMonths();
    }
}
