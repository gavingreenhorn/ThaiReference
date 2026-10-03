package com.thaireference.web;

import com.thaireference.dto.*;
import com.thaireference.repository.ThaiReadRepository;
import java.util.List;
import org.springframework.cache.annotation.Cacheable;
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
    @Cacheable("thai.sounds")
    public List<SoundDto> getSounds() {
        return repository.findSounds();
    }

    @GetMapping("/consonants")
    @Cacheable("thai.consonants")
    public List<ConsonantDto> getConsonants() {
        return repository.findConsonants();
    }

    @GetMapping("/clusters")
    @Cacheable("thai.clusters")
    public List<ClusterDto> getClusters() {
        return repository.findClusters();
    }

    @GetMapping("/homophones")
    @Cacheable("thai.homophones")
    public List<HomophoneGroupDto> getHomophones() {
        return repository.findHomophones();
    }

    @GetMapping("/minimal-sets")
    @Cacheable("thai.minimalSets")
    public List<MinimalSetDto> getMinimalSets() {
        return repository.findMinimalSets();
    }

    @GetMapping("/vowels")
    @Cacheable("thai.vowels")
    public List<VowelDto> getVowels() {
        return repository.findVowels();
    }

    @GetMapping("/marks")
    @Cacheable("thai.marks")
    public List<MarkDto> getMarks() {
        return repository.findMarks();
    }

    @GetMapping("/classes")
    @Cacheable("thai.classes")
    public List<ThaiClassDto> getClasses() {
        return repository.findClasses();
    }

    @GetMapping("/numerals")
    @Cacheable("thai.numerals")
    public List<NumeralDto> getNumerals() {
        return repository.findNumerals();
    }

    @GetMapping("/pronouns")
    @Cacheable("thai.pronouns")
    public List<PronounDto> getPronouns() {
        return repository.findPronouns();
    }

    @GetMapping("/grid-positions")
    @Cacheable("thai.gridPositions")
    public List<GridPositionCellDto> getGridPositions() {
        return repository.findGridPositions();
    }

    @GetMapping("/positions")
    @Cacheable("thai.positions")
    public List<PositionDto> getPositions() {
        return repository.findPositions();
    }

    @GetMapping("/weekdays")
    @Cacheable("thai.weekdays")
    public List<DayOfWeekDto> getWeekdays() {
        return repository.findWeekdays();
    }

    @GetMapping("/celestial-bodies")
    @Cacheable("thai.celestialBodies")
    public List<CelestialBodyDto> getCelestialBodies() {
        return repository.findCelestialBodies();
    }

    @GetMapping("/time-of-day")
    @Cacheable("thai.timeOfDay")
    public List<TimeOfDayDto> getTimeOfDay() {
        return repository.findTimeOfDay();
    }

    @GetMapping("/hours")
    @Cacheable("thai.hours")
    public List<HourDto> getHours() {
        return repository.findHours();
    }

    @GetMapping("/months")
    @Cacheable("thai.months")
    public List<MonthDto> getMonths() {
        return repository.findMonths();
    }

    @GetMapping("/directions")
    @Cacheable("thai.directions")
    public List<DirectionDto> getDirections() {
        return repository.findDirections();
    }

    @GetMapping("/colors")
    @Cacheable("thai.colors")
    public List<HueFamilyDto> getColors() {
        return repository.findColors();
    }
}
