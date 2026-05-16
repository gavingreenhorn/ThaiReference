from __future__ import annotations

from abc import ABC, abstractmethod
from collections import defaultdict

from sqlalchemy import Text, cast, func, literal, select
from sqlalchemy.engine.row import RowMapping
from sqlalchemy.orm import Session, aliased

from app.models import (
    CelestialBody,
    Class,
    Cluster,
    ClusterWord,
    CompositeName,
    Consonant,
    ConsonantSample,
    DayOfWeek,
    Ending,
    Formality,
    Gender,
    Hour,
    Lexeme,
    Month,
    Numeral,
    Plurality,
    Position,
    PositionWord,
    Pronoun,
    PronounType,
    PronounTypeBreakout,
    Sound,
    SoundType,
    SpecialSymbol,
    TimeOfDay,
    ToneMark,
    Transcription,
    Vowel,
    VowelGrapheme,
    VowelSample,
    Word,
    WordLexeme,
    Zodiac,
)
from app.schemas.thai import (
    CelestialBodyDto,
    ClusterDto,
    ConsonantDto,
    DayOfWeekDto,
    HourDto,
    MarkDto,
    MonthDto,
    NumeralDto,
    PositionCellDto,
    PronounDto,
    SoundDto,
    TimeOfDayDto,
    TranscriptionDto,
    VowelDto,
    VowelGraphemeDto,
    WordDto,
    ZodiacDto,
)


class ThaiRepository(ABC):
    """
    Database-agnostic interface for all Thai language data.
    Swap the underlying DB by providing a different implementation.
    """

    @abstractmethod
    def get_sounds(self) -> list[SoundDto]: ...

    @abstractmethod
    def get_consonants(self) -> list[ConsonantDto]: ...

    @abstractmethod
    def get_vowels(self) -> list[VowelDto]: ...

    @abstractmethod
    def get_marks(self) -> list[MarkDto]: ...

    @abstractmethod
    def get_numerals(self) -> list[NumeralDto]: ...

    @abstractmethod
    def get_pronouns(self) -> list[PronounDto]: ...

    @abstractmethod
    def get_positions(self) -> list[PositionCellDto]: ...

    @abstractmethod
    def get_weekdays(self) -> list[DayOfWeekDto]: ...

    @abstractmethod
    def get_time_of_day(self) -> list[TimeOfDayDto]: ...

    @abstractmethod
    def get_hours(self) -> list[HourDto]: ...

    @abstractmethod
    def get_months(self) -> list[MonthDto]: ...

    @abstractmethod
    def get_celestial_bodies(self) -> list[CelestialBodyDto]: ...

    @abstractmethod
    def get_clusters(self) -> list[ClusterDto]: ...


# ---------------------------------------------------------------------------
# Row-mapping helpers (RowMapping in, Pydantic DTO out)
# ---------------------------------------------------------------------------


def _word_dto(r: RowMapping, prefix: str = "") -> WordDto:
    """Build a ``WordDto`` from labelled columns. ``prefix`` lets callers
    flatten multiple words into the same row (e.g. ``initial_word_id`` etc.)."""
    return WordDto(
        id=r[f"{prefix}word_id"],
        value=(r.get(f"{prefix}word_value") or ""),
        transcription=TranscriptionDto(
            phonemic=r.get(f"{prefix}word_phonemic"),
            paiboon=r.get(f"{prefix}word_paiboon"),
            ipa=r.get(f"{prefix}word_ipa"),
        ),
        lexemes=[v for v in (r.get(f"{prefix}word_lexemes") or []) if v],
    )


def _sound_dto(r: RowMapping, prefix: str = "") -> SoundDto | None:
    """Build a ``SoundDto`` from labelled columns; returns ``None`` if no sound
    row was joined (FK was NULL)."""
    sid = r.get(f"{prefix}sound_id")
    if sid is None:
        return None
    return SoundDto(
        id=sid,
        value=(r.get(f"{prefix}sound_value") or ""),
        sound_type=(r.get(f"{prefix}sound_type_name") or ""),
        is_consonant_sound=bool(r.get(f"{prefix}sound_consonant_sound")),
        is_initial_only=bool(r.get(f"{prefix}sound_initial_only")),
        media_url=r.get(f"{prefix}sound_media_url"),
    )


def _empty_sound() -> SoundDto:
    return SoundDto(
        id=0,
        value="",
        sound_type="",
        is_consonant_sound=False,
        is_initial_only=False,
        media_url=None,
    )


# ---------------------------------------------------------------------------
# Reusable column groups
# ---------------------------------------------------------------------------


def _word_columns(prefix: str = ""):
    """Project Word + its Transcription + lexemes array. Caller must include
    a Word LEFT JOIN, an outerjoin to Transcription, an outerjoin chain
    Word -> WordLexeme -> Lexeme, and ``Word.id, Transcription.id`` in
    ``GROUP BY``."""
    return (
        Word.id.label(f"{prefix}word_id"),
        Word.value.label(f"{prefix}word_value"),
        Transcription.phonemic.label(f"{prefix}word_phonemic"),
        Transcription.paiboon.label(f"{prefix}word_paiboon"),
        Transcription.IPA.label(f"{prefix}word_ipa"),
        func.array_agg(Lexeme.value)
        .filter(Lexeme.value.isnot(None))
        .label(f"{prefix}word_lexemes"),
    )


def _sound_columns(sound_alias, sound_type_alias, prefix: str):
    """Project a Sound + sound_type name (aliased for initial/final on one row)."""
    return (
        sound_alias.id.label(f"{prefix}sound_id"),
        sound_alias.value.label(f"{prefix}sound_value"),
        sound_type_alias.name.label(f"{prefix}sound_type_name"),
        sound_alias.consonant_sound.label(f"{prefix}sound_consonant_sound"),
        sound_alias.initial_only.label(f"{prefix}sound_initial_only"),
        sound_alias.mediapath.label(f"{prefix}sound_media_url"),
    )


# ---------------------------------------------------------------------------
# SQLAlchemy projection-based implementation
# ---------------------------------------------------------------------------


class SQLAlchemyThaiRepository(ThaiRepository):
    """
    Read-only repository over the Thai schema. Every method projects scalar
    columns into RowMappings; Pydantic composes nested DTOs in Python.
    No ORM entity hydration, no SQL-side JSON building.
    """

    def __init__(self, session: Session) -> None:
        self._session = session

    # ------------------------------------------------------------------
    # Sounds
    # ------------------------------------------------------------------

    def get_sounds(self) -> list[SoundDto]:
        stmt = (
            select(
                Sound.id.label("sound_id"),
                func.coalesce(
                    SoundType.name,
                    cast(Sound.sound_type, Text),
                    literal(""),
                ).label("sound_type_name"),
                Sound.value.label("sound_value"),
                Sound.consonant_sound.label("sound_consonant_sound"),
                Sound.initial_only.label("sound_initial_only"),
                Sound.mediapath.label("sound_media_url"),
            )
            .select_from(Sound)
            .outerjoin(SoundType, SoundType.id == Sound.sound_type)
            .order_by(Sound.id)
        )
        return [
            _sound_dto(r) or _empty_sound()
            for r in self._session.execute(stmt).mappings().all()
        ]

    # ------------------------------------------------------------------
    # Consonants
    # ------------------------------------------------------------------

    def get_consonants(self) -> list[ConsonantDto]:
        # Class/Ending lookup tables: stored as VARCHAR ids on Consonant; resolve to names.
        class_map: dict[int, str] = {
            r["id"]: r["name"]
            for r in self._session.execute(select(Class.id, Class.name)).mappings().all()
        }
        ending_map: dict[int, str] = {
            r["id"]: r["name"]
            for r in self._session.execute(select(Ending.id, Ending.name)).mappings().all()
        }

        def _resolve(raw: str | None, table_map: dict[int, str]) -> str:
            try:
                return table_map.get(int(raw or "0"), raw or "")
            except (ValueError, TypeError):
                return raw or ""

        # Aliases: Consonant has both initial and final Sound joins.
        SInit = aliased(Sound)
        SFinal = aliased(Sound)
        STypeInit = aliased(SoundType)
        STypeFinal = aliased(SoundType)

        base = (
            select(
                Consonant.id.label("consonant_id"),
                Consonant.grapheme.label("grapheme"),
                Consonant.phoneme.label("phoneme"),
                Consonant.class_.label("class_raw"),
                Consonant.ending.label("ending_raw"),
                Transcription.phonemic.label("c_phonemic"),
                Transcription.paiboon.label("c_paiboon"),
                Transcription.IPA.label("c_ipa"),
                *_sound_columns(SInit, STypeInit, prefix="init_"),
                *_sound_columns(SFinal, STypeFinal, prefix="final_"),
            )
            .select_from(Consonant)
            .outerjoin(Transcription, Transcription.id == Consonant.transcription)
            .outerjoin(SInit, SInit.id == Consonant.initial)
            .outerjoin(STypeInit, STypeInit.id == SInit.sound_type)
            .outerjoin(SFinal, SFinal.id == Consonant.final_sound_id)
            .outerjoin(STypeFinal, STypeFinal.id == SFinal.sound_type)
            .order_by(Consonant.id)
        )

        samples = (
            select(
                ConsonantSample.consonant.label("consonant_id"),
                ConsonantSample.id.label("sample_id"),
                *_word_columns(),
            )
            .select_from(ConsonantSample)
            .join(Word, Word.id == ConsonantSample.word)
            .outerjoin(Transcription, Transcription.id == Word.transcription)
            .outerjoin(WordLexeme, WordLexeme.word == Word.id)
            .outerjoin(Lexeme, Lexeme.id == WordLexeme.lexeme)
            .group_by(ConsonantSample.id, Word.id, Transcription.id)
            .order_by(ConsonantSample.consonant, ConsonantSample.id)
        )

        samples_by_consonant: dict[int, list[WordDto]] = defaultdict(list)
        for r in self._session.execute(samples).mappings().all():
            samples_by_consonant[r["consonant_id"]].append(_word_dto(r))

        result: list[ConsonantDto] = []
        for r in self._session.execute(base).mappings().all():
            initial = _sound_dto(r, prefix="init_") or _empty_sound()
            final = _sound_dto(r, prefix="final_")
            result.append(
                ConsonantDto(
                    grapheme=r["grapheme"],
                    phoneme=r["phoneme"],
                    consonant_class=_resolve(r["class_raw"], class_map),
                    ending=_resolve(r["ending_raw"], ending_map),
                    transcription=TranscriptionDto(
                        phonemic=r["c_phonemic"],
                        paiboon=r["c_paiboon"],
                        ipa=r["c_ipa"],
                    ),
                    initial_sound=initial,
                    ending_sound=final,
                    sample_words=samples_by_consonant.get(r["consonant_id"], []),
                )
            )
        return result

    # ------------------------------------------------------------------
    # Vowels
    # ------------------------------------------------------------------

    def get_vowels(self) -> list[VowelDto]:
        VS = aliased(Sound)
        VSType = aliased(SoundType)

        base = (
            select(
                Vowel.id.label("vowel_id"),
                Vowel.is_diphtong.label("is_diphtong"),
                Vowel.mediapath.label("media_url"),
                *_sound_columns(VS, VSType, prefix=""),
            )
            .select_from(Vowel)
            .outerjoin(VS, VS.id == Vowel.sound_id)
            .outerjoin(VSType, VSType.id == VS.sound_type)
            .order_by(Vowel.id)
        )

        graphemes_q = (
            select(
                VowelGrapheme.vowel_id.label("vowel_id"),
                VowelGrapheme.value.label("value"),
                VowelGrapheme.is_closed.label("is_closed"),
            )
            .order_by(VowelGrapheme.vowel_id, VowelGrapheme.id)
        )

        samples_q = (
            select(
                VowelSample.vowel.label("vowel_id"),
                VowelSample.id.label("sample_id"),
                *_word_columns(),
            )
            .select_from(VowelSample)
            .join(Word, Word.id == VowelSample.word)
            .outerjoin(Transcription, Transcription.id == Word.transcription)
            .outerjoin(WordLexeme, WordLexeme.word == Word.id)
            .outerjoin(Lexeme, Lexeme.id == WordLexeme.lexeme)
            .group_by(VowelSample.id, Word.id, Transcription.id)
            .order_by(VowelSample.vowel, VowelSample.id)
        )

        graphemes_by_vowel: dict[int, list[VowelGraphemeDto]] = defaultdict(list)
        for r in self._session.execute(graphemes_q).mappings().all():
            graphemes_by_vowel[r["vowel_id"]].append(
                VowelGraphemeDto(
                    value=(r["value"] or ""),
                    is_closed=bool(r["is_closed"]),
                )
            )

        samples_by_vowel: dict[int, list[WordDto]] = defaultdict(list)
        for r in self._session.execute(samples_q).mappings().all():
            samples_by_vowel[r["vowel_id"]].append(_word_dto(r))

        result: list[VowelDto] = []
        for r in self._session.execute(base).mappings().all():
            sound = _sound_dto(r)
            if sound is None:
                continue
            result.append(
                VowelDto(
                    is_diphtong=bool(r["is_diphtong"]),
                    sound=sound,
                    media_url=r["media_url"],
                    graphemes=graphemes_by_vowel.get(r["vowel_id"], []),
                    sample_words=samples_by_vowel.get(r["vowel_id"], []),
                )
            )
        return result

    # ------------------------------------------------------------------
    # Marks (SpecialSymbol + ToneMark)
    # ------------------------------------------------------------------

    def get_marks(self) -> list[MarkDto]:
        # SpecialSymbol: one row per symbol, single Word.
        ss_q = (
            select(
                SpecialSymbol.id.label("ss_id"),
                SpecialSymbol.grapheme.label("grapheme"),
                *_word_columns(),
            )
            .select_from(SpecialSymbol)
            .outerjoin(Word, Word.id == SpecialSymbol.word_id)
            .outerjoin(Transcription, Transcription.id == Word.transcription)
            .outerjoin(WordLexeme, WordLexeme.word == Word.id)
            .outerjoin(Lexeme, Lexeme.id == WordLexeme.lexeme)
            .group_by(SpecialSymbol.id, Word.id, Transcription.id)
            .order_by(SpecialSymbol.id)
        )

        # ToneMark + CompositeName: 5 word ids per row.
        tm_q = (
            select(
                ToneMark.id.label("tm_id"),
                ToneMark.mark.label("mark"),
                ToneMark.english_label.label("english_label"),
                CompositeName.word1.label("w1"),
                CompositeName.word2.label("w2"),
                CompositeName.word3.label("w3"),
                CompositeName.word4.label("w4"),
                CompositeName.word5.label("w5"),
            )
            .select_from(ToneMark)
            .outerjoin(CompositeName, CompositeName.id == ToneMark.name)
            .order_by(ToneMark.id)
        )

        result: list[MarkDto] = []
        for r in self._session.execute(ss_q).mappings().all():
            words: list[WordDto] = []
            if r.get("word_id") is not None:
                words.append(_word_dto(r))
            result.append(
                MarkDto(
                    grapheme=(r["grapheme"] or ""),
                    tone_mark=False,
                    words=words,
                )
            )

        tm_rows = self._session.execute(tm_q).mappings().all()

        # Collect every word id referenced across all tone marks, fetch in one shot.
        word_ids: set[int] = set()
        for r in tm_rows:
            for k in ("w1", "w2", "w3", "w4", "w5"):
                wid = r.get(k)
                if wid is not None:
                    word_ids.add(wid)

        word_map: dict[int, WordDto] = {}
        if word_ids:
            words_q = (
                select(*_word_columns())
                .select_from(Word)
                .outerjoin(Transcription, Transcription.id == Word.transcription)
                .outerjoin(WordLexeme, WordLexeme.word == Word.id)
                .outerjoin(Lexeme, Lexeme.id == WordLexeme.lexeme)
                .where(Word.id.in_(word_ids))
                .group_by(Word.id, Transcription.id)
            )
            word_map = {
                r["word_id"]: _word_dto(r)
                for r in self._session.execute(words_q).mappings().all()
            }

        for r in tm_rows:
            words = []
            for k in ("w1", "w2", "w3", "w4", "w5"):
                wid = r.get(k)
                if wid is not None and wid in word_map:
                    words.append(word_map[wid])
            result.append(
                MarkDto(
                    grapheme=(r["mark"] or ""),
                    tone_mark=True,
                    words=words,
                    english_label=r.get("english_label"),
                )
            )

        return result

    # ------------------------------------------------------------------
    # Numerals
    # ------------------------------------------------------------------

    def get_numerals(self) -> list[NumeralDto]:
        stmt = (
            select(
                Numeral.id.label("numeral_id"),
                Numeral.grapheme.label("grapheme"),
                *_word_columns(),
            )
            .select_from(Numeral)
            .join(Word, Word.id == Numeral.word)
            .outerjoin(Transcription, Transcription.id == Word.transcription)
            .outerjoin(WordLexeme, WordLexeme.word == Word.id)
            .outerjoin(Lexeme, Lexeme.id == WordLexeme.lexeme)
            .group_by(Numeral.id, Word.id, Transcription.id)
            .order_by(Numeral.id)
        )
        return [
            NumeralDto(
                grapheme=(r["grapheme"] or ""),
                name=_word_dto(r),
            )
            for r in self._session.execute(stmt).mappings().all()
        ]

    # ------------------------------------------------------------------
    # Pronouns
    # ------------------------------------------------------------------

    def get_pronouns(self) -> list[PronounDto]:
        base = (
            select(
                Pronoun.id.label("pronoun_id"),
                Pronoun.grammatical_person.label("person"),
                Plurality.name.label("plurality_name"),
                Formality.name.label("formality_name"),
                Gender.name.label("gender_name"),
                *_word_columns(),
            )
            .select_from(Pronoun)
            .join(Word, Word.id == Pronoun.word)
            .outerjoin(Transcription, Transcription.id == Word.transcription)
            .outerjoin(WordLexeme, WordLexeme.word == Word.id)
            .outerjoin(Lexeme, Lexeme.id == WordLexeme.lexeme)
            .outerjoin(Plurality, Plurality.id == Pronoun.plurality)
            .outerjoin(Formality, Formality.id == Pronoun.formality)
            .outerjoin(Gender, Gender.id == Pronoun.gender)
            .group_by(
                Pronoun.id,
                Word.id,
                Transcription.id,
                Plurality.id,
                Formality.id,
                Gender.id,
            )
            .order_by(Pronoun.id)
        )

        types_q = (
            select(
                PronounTypeBreakout.pronoun_id.label("pronoun_id"),
                PronounType.name.label("type_name"),
            )
            .select_from(PronounTypeBreakout)
            .join(PronounType, PronounType.id == PronounTypeBreakout.type_id)
            .order_by(PronounTypeBreakout.pronoun_id, PronounTypeBreakout.id)
        )

        types_by_pronoun: dict[int, list[str]] = defaultdict(list)
        for r in self._session.execute(types_q).mappings().all():
            name = r["type_name"]
            if name:
                types_by_pronoun[r["pronoun_id"]].append(name)

        def _plurality_int(name: str | None) -> int:
            if not name:
                return 0
            n = name.lower()
            if "singular" in n:
                return 1
            if "plural" in n:
                return 2
            return 3

        result: list[PronounDto] = []
        for r in self._session.execute(base).mappings().all():
            result.append(
                PronounDto(
                    name=_word_dto(r),
                    person=(r["person"] or 0),
                    plurality=_plurality_int(r["plurality_name"]),
                    formality=(r["formality_name"] or ""),
                    gender=(r["gender_name"] or ""),
                    types=types_by_pronoun.get(r["pronoun_id"], []),
                )
            )
        return result

    # ------------------------------------------------------------------
    # Positions (grid)
    # ------------------------------------------------------------------

    def get_positions(self) -> list[PositionCellDto]:
        base = (
            select(
                Position.id.label("position_id"),
                Position.row_index.label("row_index"),
                Position.col_index.label("col_index"),
                Position.tile_style.label("tile_style"),
                Position.name.label("label"),
            )
            .select_from(Position)
            .order_by(Position.row_index, Position.col_index)
        )

        words_q = (
            select(
                PositionWord.position_id.label("position_id"),
                PositionWord.word_order.label("word_order"),
                *_word_columns(),
            )
            .select_from(PositionWord)
            .join(Word, Word.id == PositionWord.word_id)
            .outerjoin(Transcription, Transcription.id == Word.transcription)
            .outerjoin(WordLexeme, WordLexeme.word == Word.id)
            .outerjoin(Lexeme, Lexeme.id == WordLexeme.lexeme)
            .group_by(PositionWord.id, Word.id, Transcription.id)
            .order_by(PositionWord.position_id, PositionWord.word_order)
        )

        words_by_position: dict[int, list[WordDto]] = defaultdict(list)
        for r in self._session.execute(words_q).mappings().all():
            words_by_position[r["position_id"]].append(_word_dto(r))

        return [
            PositionCellDto(
                ordinal=r["position_id"],
                row=r["row_index"],
                col=r["col_index"],
                tile_style=r["tile_style"],
                label=r["label"],
                words=words_by_position.get(r["position_id"], []),
            )
            for r in self._session.execute(base).mappings().all()
        ]

    # ------------------------------------------------------------------
    # Weekdays / TimeOfDay / Hour / Month / CelestialBody
    # ------------------------------------------------------------------

    def get_weekdays(self) -> list[DayOfWeekDto]:
        stmt = (
            select(
                DayOfWeek.ordinal.label("ordinal"),
                DayOfWeek.deity.label("deity"),
                DayOfWeek.image_url.label("image_url"),
                *_word_columns(),
            )
            .select_from(DayOfWeek)
            .join(Word, Word.id == DayOfWeek.word_id)
            .outerjoin(Transcription, Transcription.id == Word.transcription)
            .outerjoin(WordLexeme, WordLexeme.word == Word.id)
            .outerjoin(Lexeme, Lexeme.id == WordLexeme.lexeme)
            .group_by(DayOfWeek.id, Word.id, Transcription.id)
            .order_by(DayOfWeek.ordinal)
        )
        return [
            DayOfWeekDto(
                ordinal=r["ordinal"],
                word=_word_dto(r),
                deity=r.get("deity"),
                image_url=r.get("image_url"),
            )
            for r in self._session.execute(stmt).mappings().all()
        ]

    def get_time_of_day(self) -> list[TimeOfDayDto]:
        stmt = (
            select(
                TimeOfDay.ordinal.label("ordinal"),
                *_word_columns(),
            )
            .select_from(TimeOfDay)
            .join(Word, Word.id == TimeOfDay.word_id)
            .outerjoin(Transcription, Transcription.id == Word.transcription)
            .outerjoin(WordLexeme, WordLexeme.word == Word.id)
            .outerjoin(Lexeme, Lexeme.id == WordLexeme.lexeme)
            .group_by(TimeOfDay.id, Word.id, Transcription.id)
            .order_by(TimeOfDay.ordinal)
        )
        return [
            TimeOfDayDto(ordinal=r["ordinal"], word=_word_dto(r))
            for r in self._session.execute(stmt).mappings().all()
        ]

    def get_hours(self) -> list[HourDto]:
        stmt = (
            select(
                Hour.ordinal.label("ordinal"),
                *_word_columns(),
            )
            .select_from(Hour)
            .join(Word, Word.id == Hour.word_id)
            .outerjoin(Transcription, Transcription.id == Word.transcription)
            .outerjoin(WordLexeme, WordLexeme.word == Word.id)
            .outerjoin(Lexeme, Lexeme.id == WordLexeme.lexeme)
            .group_by(Hour.id, Word.id, Transcription.id)
            .order_by(Hour.ordinal)
        )
        return [
            HourDto(ordinal=r["ordinal"], word=_word_dto(r))
            for r in self._session.execute(stmt).mappings().all()
        ]

    def get_months(self) -> list[MonthDto]:
        stmt = (
            select(
                Month.ordinal.label("ordinal"),
                Month.short_name.label("short_name"),
                Month.english_name.label("english_name"),
                Zodiac.ordinal.label("z_ordinal"),
                Zodiac.english_name.label("z_english_name"),
                Zodiac.sanskrit_name.label("z_sanskrit_name"),
                Zodiac.sanskrit_lexeme.label("z_sanskrit_lexeme"),
                Zodiac.element.label("z_element"),
                Zodiac.image_url.label("z_image_url"),
                *_word_columns(),
            )
            .select_from(Month)
            .join(Word, Word.id == Month.word_id)
            .outerjoin(Transcription, Transcription.id == Word.transcription)
            .outerjoin(WordLexeme, WordLexeme.word == Word.id)
            .outerjoin(Lexeme, Lexeme.id == WordLexeme.lexeme)
            .join(Zodiac, Zodiac.id == Month.zodiac_id)
            .group_by(Month.id, Word.id, Transcription.id, Zodiac.id)
            .order_by(Month.ordinal)
        )
        return [
            MonthDto(
                ordinal=r["ordinal"],
                word=_word_dto(r),
                short_name=r["short_name"],
                english_name=r["english_name"],
                zodiac=ZodiacDto(
                    ordinal=r["z_ordinal"],
                    english_name=r["z_english_name"],
                    sanskrit_name=r["z_sanskrit_name"],
                    sanskrit_lexeme=r["z_sanskrit_lexeme"],
                    element=r["z_element"],
                    image_url=r["z_image_url"],
                ),
            )
            for r in self._session.execute(stmt).mappings().all()
        ]

    def get_celestial_bodies(self) -> list[CelestialBodyDto]:
        W = aliased(Word)
        Tr = aliased(Transcription)
        Wl = aliased(WordLexeme)
        Lm = aliased(Lexeme)
        stmt = (
            select(
                CelestialBody.ordinal,
                CelestialBody.glyph,
                CelestialBody.color_hex,
                W.id.label("word_id"),
                W.value.label("word_value"),
                Tr.phonemic.label("word_phonemic"),
                Tr.paiboon.label("word_paiboon"),
                Tr.IPA.label("word_ipa"),
                func.array_agg(Lm.value)
                .filter(Lm.value.isnot(None))
                .label("word_lexemes"),
            )
            .select_from(CelestialBody)
            .join(W, W.id == CelestialBody.word_id)
            .outerjoin(Tr, Tr.id == W.transcription)
            .outerjoin(Wl, Wl.word == W.id)
            .outerjoin(Lm, Lm.id == Wl.lexeme)
            .group_by(
                CelestialBody.id,
                CelestialBody.ordinal,
                CelestialBody.glyph,
                CelestialBody.color_hex,
                W.id,
                W.value,
                Tr.id,
                Tr.phonemic,
                Tr.paiboon,
                Tr.IPA,
            )
            .order_by(CelestialBody.ordinal)
        )
        return [
            CelestialBodyDto(
                ordinal=r["ordinal"],
                glyph=r["glyph"],
                color_hex=r["color_hex"],
                word=_word_dto(r),
            )
            for r in self._session.execute(stmt).mappings().all()
        ]

    def get_clusters(self) -> list[ClusterDto]:
        cluster_stmt = select(Cluster).order_by(
            Cluster.type, Cluster.subtype.nulls_last(), Cluster.value, Cluster.id
        )
        clusters = list(self._session.scalars(cluster_stmt).all())

        cw_stmt = (
            select(
                ClusterWord.cluster_id,
                ClusterWord.id.label("cluster_word_row_id"),
                *_word_columns(),
            )
            .select_from(ClusterWord)
            .join(Word, Word.id == ClusterWord.word_id)
            .outerjoin(Transcription, Transcription.id == Word.transcription)
            .outerjoin(WordLexeme, WordLexeme.word == Word.id)
            .outerjoin(Lexeme, Lexeme.id == WordLexeme.lexeme)
            .group_by(ClusterWord.id, ClusterWord.cluster_id, Word.id, Transcription.id)
            .order_by(ClusterWord.cluster_id, ClusterWord.id)
        )
        by_cluster: dict[int, list[tuple[int, WordDto]]] = defaultdict(list)
        for r in self._session.execute(cw_stmt).mappings().all():
            cid = int(r["cluster_id"])
            by_cluster[cid].append((int(r["cluster_word_row_id"]), _word_dto(r)))

        return [
            ClusterDto(
                id=c.id,
                type=c.type,
                subtype=c.subtype,
                value=c.value,
                sample_words=[w for _, w in sorted(by_cluster.get(c.id, []), key=lambda x: x[0])],
            )
            for c in clusters
        ]
