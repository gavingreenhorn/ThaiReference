"""Single router for all Thai read-only data endpoints."""

from fastapi import APIRouter

from app.core.deps import RepoDep
from app.schemas.thai import (
    CalendarPeriodDto,
    ConsonantDto,
    HourDto,
    MarkDto,
    MonthDto,
    NumeralDto,
    PositionCellDto,
    PronounDto,
    SoundDto,
    TimeOfDayDto,
    VowelDto,
)

router = APIRouter(tags=["thai"])


@router.get("/sounds", response_model=list[SoundDto])
def get_sounds(repo: RepoDep) -> list[SoundDto]:
    return repo.get_sounds()


@router.get("/consonants", response_model=list[ConsonantDto])
def get_consonants(repo: RepoDep) -> list[ConsonantDto]:
    return repo.get_consonants()


@router.get("/vowels", response_model=list[VowelDto])
def get_vowels(repo: RepoDep) -> list[VowelDto]:
    return repo.get_vowels()


@router.get("/marks", response_model=list[MarkDto])
def get_marks(repo: RepoDep) -> list[MarkDto]:
    return repo.get_marks()


@router.get("/numerals", response_model=list[NumeralDto])
def get_numerals(repo: RepoDep) -> list[NumeralDto]:
    return repo.get_numerals()


@router.get("/pronouns", response_model=list[PronounDto])
def get_pronouns(repo: RepoDep) -> list[PronounDto]:
    return repo.get_pronouns()


@router.get("/positions", response_model=list[PositionCellDto])
def get_positions(repo: RepoDep) -> list[PositionCellDto]:
    return repo.get_positions()


@router.get("/calendar", response_model=list[CalendarPeriodDto])
def get_calendar_periods(repo: RepoDep) -> list[CalendarPeriodDto]:
    return repo.get_calendar_periods()


@router.get("/time-of-day", response_model=list[TimeOfDayDto])
def get_time_of_day(repo: RepoDep) -> list[TimeOfDayDto]:
    return repo.get_time_of_day()


@router.get("/hours", response_model=list[HourDto])
def get_hours(repo: RepoDep) -> list[HourDto]:
    return repo.get_hours()


@router.get("/months", response_model=list[MonthDto])
def get_months(repo: RepoDep) -> list[MonthDto]:
    return repo.get_months()
