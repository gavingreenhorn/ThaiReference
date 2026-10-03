package com.thaireference.repository;

import com.thaireference.dto.*;
import java.util.List;

public interface ThaiReadRepositoryCustom {

    List<NumeralDto> findNumerals();

    List<DayOfWeekDto> findWeekdays();

    List<TimeOfDayDto> findTimeOfDay();

    List<HourDto> findHours();

    List<MonthDto> findMonths();

    List<CelestialBodyDto> findCelestialBodies();

    List<ConsonantDto> findConsonants();

    List<VowelDto> findVowels();

    List<MarkDto> findMarks();

    List<ThaiClassDto> findClasses();

    List<PronounDto> findPronouns();

    List<GridPositionCellDto> findGridPositions();

    List<ClusterDto> findClusters();

    List<HomophoneGroupDto> findHomophones();

    List<MinimalSetDto> findMinimalSets();

    List<DirectionDto> findDirections();

    List<PositionDto> findPositions();

    List<HueFamilyDto> findColors();
}
