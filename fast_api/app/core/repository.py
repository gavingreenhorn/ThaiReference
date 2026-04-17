from abc import ABC, abstractmethod

from sqlalchemy.orm import Session, joinedload, selectinload

from app.models.orm_tables import (
    ClassOrm,
    ConsonantOrm,
    ConsonantSampleOrm,
    EndingOrm,
    NumeralOrm,
    PronounOrm,
    PronounTypeBreakoutOrm,
    SoundOrm,
    SpecialSymbolOrm,
    ToneMarkOrm,
    TranscriptionOrm,
    VowelOrm,
    VowelSampleOrm,
    WordLexemeOrm,
    WordOrm,
)
from app.schemas.thai import (
    ConsonantDto,
    MarkDto,
    NumeralDto,
    PronounDto,
    SoundDto,
    TranscriptionDto,
    VowelDto,
    VowelGraphemeDto,
    WordDto,
)
from sqlalchemy import select


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


# ---------------------------------------------------------------------------
# Helpers: ORM -> DTO
# ---------------------------------------------------------------------------


def _transcription_dto(t: TranscriptionOrm | None) -> TranscriptionDto:
    if t is None:
        return TranscriptionDto()
    return TranscriptionDto(
        phonemic=t.phonemic,
        paiboon=t.paiboon,
        ipa=t.IPA,
    )


def _word_dto(w: WordOrm, lexeme_values: list[str] | None = None) -> WordDto:
    if lexeme_values is None:
        lexeme_values = []
        if w.word_lexemes:
            lexeme_values = [
                le.lexeme_rel.value or ""
                for le in w.word_lexemes
                if le.lexeme_rel
            ]
    return WordDto(
        id=w.id,
        value=w.value or "",
        media=None,
        transcription=_transcription_dto(w.transcription_rel),
        lexemes=lexeme_values,
    )


def _sound_dto(s: SoundOrm) -> SoundDto:
    return SoundDto(
        id=s.id,
        sound_type=s.sound_type_rel.name if s.sound_type_rel else (str(s.sound_type) if s.sound_type is not None else ""),
        is_consonant_sound=bool(s.consonant_sound),
        is_initial_only=bool(s.initial_only),
        media_url=s.mediapath,
        media=None,
        transcription=_transcription_dto(s.transcription_rel),
    )


# ---------------------------------------------------------------------------
# ORM-based implementation
# ---------------------------------------------------------------------------


class SQLAlchemyThaiRepository(ThaiRepository):
    """
    Uses ORM models for Thai reference data. Loads entities with
    relationships and converts to DTOs. No raw SQL.
    """

    def __init__(self, session: Session) -> None:
        self._session = session

    def get_sounds(self) -> list[SoundDto]:
        stmt = (
            select(SoundOrm)
            .options(
                joinedload(SoundOrm.transcription_rel),
                joinedload(SoundOrm.sound_type_rel),
            )
        )
        rows = self._session.execute(stmt).scalars().unique().all()
        return [_sound_dto(s) for s in rows]

    def get_consonants(self) -> list[ConsonantDto]:
        stmt = (
            select(ConsonantOrm)
            .options(
                joinedload(ConsonantOrm.transcription_rel),
                joinedload(ConsonantOrm.initial_sound_rel).joinedload(SoundOrm.transcription_rel),
                joinedload(ConsonantOrm.initial_sound_rel).joinedload(SoundOrm.sound_type_rel),
                joinedload(ConsonantOrm.final_sound_rel).joinedload(SoundOrm.transcription_rel),
                joinedload(ConsonantOrm.final_sound_rel).joinedload(SoundOrm.sound_type_rel),
                selectinload(ConsonantOrm.sample_links).joinedload(ConsonantSampleOrm.word_rel).joinedload(WordOrm.transcription_rel),
                selectinload(ConsonantOrm.sample_links).joinedload(ConsonantSampleOrm.word_rel).selectinload(WordOrm.word_lexemes).joinedload(WordLexemeOrm.lexeme_rel),
            )
        )
        rows = self._session.execute(stmt).scalars().unique().all()
        # Resolve class and ending IDs to names (ConsonantsView exposes names; table stores IDs)
        class_map = {r.id: (r.name or "") for r in self._session.execute(select(ClassOrm)).scalars().all()}
        ending_map = {r.id: (r.name or "") for r in self._session.execute(select(EndingOrm)).scalars().all()}

        def _class_name(raw: str) -> str:
            try:
                return class_map.get(int(raw), raw)
            except (ValueError, TypeError):
                return raw or ""

        def _ending_name(raw: str) -> str:
            try:
                return ending_map.get(int(raw), raw)
            except (ValueError, TypeError):
                return raw or ""

        result = []
        for c in rows:
            sample_words = []
            for link in c.sample_links or []:
                if link.word_rel:
                    sample_words.append(_word_dto(link.word_rel))
            initial = c.initial_sound_rel
            ending_sound = c.final_sound_rel
            default_sound = SoundDto(
                id=0,
                sound_type="",
                is_consonant_sound=False,
                is_initial_only=False,
                media_url=None,
                media=None,
                transcription=TranscriptionDto(),
            )
            result.append(
                ConsonantDto(
                    grapheme=c.grapheme,
                    phoneme=c.phoneme,
                    consonant_class=_class_name(str(c.class_)),
                    ending=_ending_name(str(c.ending)),
                    media=None,
                    transcription=_transcription_dto(c.transcription_rel),
                    initial_sound=_sound_dto(initial) if initial else default_sound,
                    ending_sound=_sound_dto(ending_sound) if ending_sound else None,
                    sample_words=sample_words,
                )
            )
        return result

    def get_vowels(self) -> list[VowelDto]:
        stmt = (
            select(VowelOrm)
            .options(
                joinedload(VowelOrm.sound_rel).joinedload(SoundOrm.transcription_rel),
                joinedload(VowelOrm.sound_rel).joinedload(SoundOrm.sound_type_rel),
                joinedload(VowelOrm.graphemes),
                selectinload(VowelOrm.sample_links).joinedload(VowelSampleOrm.word_rel).joinedload(WordOrm.transcription_rel),
                selectinload(VowelOrm.sample_links).joinedload(VowelSampleOrm.word_rel).selectinload(WordOrm.word_lexemes).joinedload(WordLexemeOrm.lexeme_rel),
            )
        )
        rows = self._session.execute(stmt).scalars().unique().all()
        result = []
        for v in rows:
            sound = v.sound_rel
            graphemes = [
                VowelGraphemeDto(value=g.value or "", is_closed=bool(g.is_closed))
                for g in (v.graphemes or [])
            ]
            sample_words = []
            for link in v.sample_links or []:
                if link.word_rel:
                    sample_words.append(_word_dto(link.word_rel))
            if sound:
                result.append(
                    VowelDto(
                        is_diphtong=bool(v.is_diphtong),
                        sound=_sound_dto(sound),
                        media_url=sound.mediapath,
                        graphemes=graphemes,
                        sample_words=sample_words,
                    )
                )
        return result

    def get_marks(self) -> list[MarkDto]:
        result = []
        # SpecialSymbol: non-tone marks
        stmt_ss = (
            select(SpecialSymbolOrm)
            .options(
                joinedload(SpecialSymbolOrm.word_rel).joinedload(WordOrm.transcription_rel),
                joinedload(SpecialSymbolOrm.word_rel).selectinload(WordOrm.word_lexemes).joinedload(WordLexemeOrm.lexeme_rel),
            )
        )
        for ss in self._session.execute(stmt_ss).scalars().unique().all():
            words = [_word_dto(ss.word_rel)] if ss.word_rel else []
            result.append(
                MarkDto(
                    grapheme=ss.grapheme or "",
                    tone_mark=False,
                    media=None,
                    words=words,
                )
            )
        # ToneMark: tone marks with CompositeName -> words
        stmt_tm = (
            select(ToneMarkOrm)
            .options(
                joinedload(ToneMarkOrm.name_rel),
            )
        )
        for tm in self._session.execute(stmt_tm).scalars().unique().all():
            words = []
            if tm.name_rel:
                comp = tm.name_rel
                word_ids = [comp.word1, comp.word2, comp.word3, comp.word4, comp.word5]
                word_ids = [wid for wid in word_ids if wid is not None]
                if word_ids:
                    stmt_w = (
                        select(WordOrm)
                        .where(WordOrm.id.in_(word_ids))
                        .options(
                            joinedload(WordOrm.transcription_rel),
                            selectinload(WordOrm.word_lexemes).joinedload(WordLexemeOrm.lexeme_rel),
                        )
                    )
                    word_map = {w.id: w for w in self._session.execute(stmt_w).scalars().unique().all()}
                    for wid in word_ids:
                        if wid in word_map:
                            words.append(_word_dto(word_map[wid]))
            result.append(
                MarkDto(
                    grapheme=tm.mark or "",
                    tone_mark=True,
                    media=None,
                    words=words,
                )
            )
        return result

    def get_numerals(self) -> list[NumeralDto]:
        stmt = (
            select(NumeralOrm)
            .options(
                joinedload(NumeralOrm.word_rel).joinedload(WordOrm.transcription_rel),
                joinedload(NumeralOrm.word_rel).selectinload(WordOrm.word_lexemes).joinedload(WordLexemeOrm.lexeme_rel),
            )
        )
        rows = self._session.execute(stmt).scalars().unique().all()
        result = []
        for n in rows:
            if n.word_rel:
                result.append(
                    NumeralDto(
                        grapheme=n.grapheme or "",
                        name=_word_dto(n.word_rel),
                    )
                )
        return result

    def get_pronouns(self) -> list[PronounDto]:
        stmt = (
            select(PronounOrm)
            .options(
                joinedload(PronounOrm.word_rel).joinedload(WordOrm.transcription_rel),
                joinedload(PronounOrm.word_rel).selectinload(WordOrm.word_lexemes).joinedload(WordLexemeOrm.lexeme_rel),
                joinedload(PronounOrm.gender_rel),
                joinedload(PronounOrm.formality_rel),
                joinedload(PronounOrm.plurality_rel),
            )
        )
        rows = self._session.execute(stmt).scalars().unique().all()
        result = []
        for p in rows:
            if not p.word_rel:
                continue
            types = []
            from app.models.orm_tables import PronounTypeOrm
            stmt_ptb = select(PronounTypeBreakoutOrm).where(PronounTypeBreakoutOrm.pronoun_id == p.id)
            for ptb in self._session.execute(stmt_ptb).scalars().all():
                if ptb.type_id is not None:
                    type_row = self._session.get(PronounTypeOrm, ptb.type_id)
                    if type_row and type_row.name:
                        types.append(type_row.name)
            plurality_val = 0
            if p.plurality_rel and p.plurality_rel.name:
                if "singular" in (p.plurality_rel.name or "").lower():
                    plurality_val = 1
                elif "plural" in (p.plurality_rel.name or "").lower():
                    plurality_val = 2
                else:
                    plurality_val = 3
            result.append(
                PronounDto(
                    name=_word_dto(p.word_rel),
                    person=p.grammatical_person or 0,
                    plurality=plurality_val,
                    formality=p.formality_rel.name if p.formality_rel else "",
                    gender=p.gender_rel.name if p.gender_rel else "",
                    types=types,
                )
            )
        return result
