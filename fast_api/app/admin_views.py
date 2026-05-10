"""SQLAdmin ModelViews for Thai reference tables."""
from sqladmin import ModelView

from app.models import (
    CelestialBody,
    Class,
    CompositeName,
    Consonant,
    ConsonantSample,
    DayOfWeek,
    Ending,
    Formality,
    Gender,
    Lexeme,
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
)


class _ThaiModelView(ModelView):
    """Base admin view with create/edit/delete/details enabled."""
    can_create = True
    can_edit = True
    can_delete = True
    can_view_details = True


# Reference
class TranscriptionAdmin(_ThaiModelView, model=Transcription):
    name = "Transcription"
    category = "Reference"
    column_list = [Transcription.id, Transcription.phonemic, Transcription.paiboon, Transcription.IPA]


class SoundTypeAdmin(_ThaiModelView, model=SoundType):
    name = "Sound Type"
    category = "Reference"
    column_list = [SoundType.id, SoundType.name]


class LexemeAdmin(_ThaiModelView, model=Lexeme):
    name = "Lexeme"
    category = "Reference"
    column_list = [Lexeme.id, Lexeme.value]


class ClassAdmin(_ThaiModelView, model=Class):
    name = "Class"
    category = "Reference"
    column_list = [Class.id, Class.name]


class EndingAdmin(_ThaiModelView, model=Ending):
    name = "Ending"
    category = "Reference"
    column_list = [Ending.id, Ending.name]


class GenderAdmin(_ThaiModelView, model=Gender):
    name = "Gender"
    category = "Reference"
    column_list = [Gender.id, Gender.name]


class FormalityAdmin(_ThaiModelView, model=Formality):
    name = "Formality"
    category = "Reference"
    column_list = [Formality.id, Formality.name]


class PluralityAdmin(_ThaiModelView, model=Plurality):
    name = "Plurality"
    category = "Reference"
    column_list = [Plurality.id, Plurality.name]


class PronounTypeAdmin(_ThaiModelView, model=PronounType):
    name = "Pronoun Type"
    category = "Reference"
    column_list = [PronounType.id, PronounType.name]


# Content


class CelestialBodyAdmin(_ThaiModelView, model=CelestialBody):
    name = "Celestial body"
    category = "Content"
    column_list = [
        CelestialBody.id,
        CelestialBody.ordinal,
        CelestialBody.glyph,
        CelestialBody.color_hex,
        CelestialBody.word_id,
    ]


class DayOfWeekAdmin(_ThaiModelView, model=DayOfWeek):
    name = "Day of week"
    category = "Content"
    column_list = [
        DayOfWeek.id,
        DayOfWeek.ordinal,
        DayOfWeek.word_id,
        DayOfWeek.celestial_body_id,
        DayOfWeek.deity,
        DayOfWeek.image_url,
    ]


class TimeOfDayAdmin(_ThaiModelView, model=TimeOfDay):
    name = "Time of day"
    category = "Content"
    column_list = [TimeOfDay.id, TimeOfDay.ordinal, TimeOfDay.word_id]


class WordAdmin(_ThaiModelView, model=Word):
    name = "Word"
    category = "Content"
    column_list = [Word.id, Word.transcription, Word.value]


class WordLexemeAdmin(_ThaiModelView, model=WordLexeme):
    name = "Word-Lexeme"
    category = "Associations"
    column_list = [WordLexeme.id, WordLexeme.word, WordLexeme.lexeme]


class PositionAdmin(_ThaiModelView, model=Position):
    name = "Position (grid cell)"
    category = "Content"
    column_list = [
        Position.id,
        Position.row_index,
        Position.col_index,
        Position.name,
        Position.tile_style,
    ]


class PositionWordAdmin(_ThaiModelView, model=PositionWord):
    name = "Position word"
    category = "Associations"
    column_list = [
        PositionWord.id,
        PositionWord.position_id,
        PositionWord.word_id,
        PositionWord.word_order,
    ]


class SoundAdmin(_ThaiModelView, model=Sound):
    name = "Sound"
    category = "Content"
    column_list = [
        Sound.id,
        Sound.transcription,
        Sound.mediapath,
        Sound.initial_only,
        Sound.consonant_sound,
        Sound.sound_type,
    ]


class ConsonantAdmin(_ThaiModelView, model=Consonant):
    name = "Consonant"
    category = "Content"
    column_list = [
        Consonant.id,
        Consonant.grapheme,
        Consonant.phoneme,
        Consonant.soundtype,
        Consonant.class_,
        Consonant.ending,
        Consonant.transcription,
        Consonant.acrophonic,
        Consonant.mediapath,
        Consonant.initial,
        Consonant.final_sound_id,
    ]


class ConsonantSampleAdmin(_ThaiModelView, model=ConsonantSample):
    name = "Consonant Sample"
    category = "Associations"
    column_list = [ConsonantSample.id, ConsonantSample.consonant, ConsonantSample.word]


class VowelAdmin(_ThaiModelView, model=Vowel):
    name = "Vowel"
    category = "Content"
    column_list = [Vowel.id, Vowel.sound_id, Vowel.is_diphtong, Vowel.mediapath]


class VowelGraphemeAdmin(_ThaiModelView, model=VowelGrapheme):
    name = "Vowel Grapheme"
    category = "Associations"
    column_list = [VowelGrapheme.id, VowelGrapheme.vowel_id, VowelGrapheme.value, VowelGrapheme.is_closed]


class VowelSampleAdmin(_ThaiModelView, model=VowelSample):
    name = "Vowel Sample"
    category = "Associations"
    column_list = [VowelSample.id, VowelSample.vowel, VowelSample.word]


class NumeralAdmin(_ThaiModelView, model=Numeral):
    name = "Numeral"
    category = "Content"
    column_list = [Numeral.id, Numeral.word, Numeral.grapheme]


class PronounAdmin(_ThaiModelView, model=Pronoun):
    name = "Pronoun"
    category = "Content"
    column_list = [
        Pronoun.id,
        Pronoun.word,
        Pronoun.gender,
        Pronoun.formality,
        Pronoun.plurality,
        Pronoun.grammatical_person,
    ]


class PronounTypeBreakoutAdmin(_ThaiModelView, model=PronounTypeBreakout):
    name = "Pronoun Type Breakout"
    category = "Associations"
    column_list = [PronounTypeBreakout.id, PronounTypeBreakout.pronoun_id, PronounTypeBreakout.type_id]


class SpecialSymbolAdmin(_ThaiModelView, model=SpecialSymbol):
    name = "Special Symbol"
    category = "Content"
    column_list = [SpecialSymbol.id, SpecialSymbol.grapheme, SpecialSymbol.word_id]


class CompositeNameAdmin(_ThaiModelView, model=CompositeName):
    name = "Composite Name"
    category = "Content"
    column_list = [
        CompositeName.id,
        CompositeName.word1,
        CompositeName.word2,
        CompositeName.word3,
        CompositeName.word4,
        CompositeName.word5,
    ]


class ToneMarkAdmin(_ThaiModelView, model=ToneMark):
    name = "Tone Mark"
    category = "Content"
    column_list = [ToneMark.id, ToneMark.mark, ToneMark.name, ToneMark.mediapath]


def get_admin_views():
    """Return all admin view classes in menu order."""
    return [
        TranscriptionAdmin,
        SoundTypeAdmin,
        LexemeAdmin,
        ClassAdmin,
        EndingAdmin,
        GenderAdmin,
        FormalityAdmin,
        PluralityAdmin,
        PronounTypeAdmin,
        WordAdmin,
        CelestialBodyAdmin,
        DayOfWeekAdmin,
        TimeOfDayAdmin,
        WordLexemeAdmin,
        PositionAdmin,
        PositionWordAdmin,
        SoundAdmin,
        ConsonantAdmin,
        ConsonantSampleAdmin,
        VowelAdmin,
        VowelGraphemeAdmin,
        VowelSampleAdmin,
        NumeralAdmin,
        PronounAdmin,
        PronounTypeBreakoutAdmin,
        SpecialSymbolAdmin,
        CompositeNameAdmin,
        ToneMarkAdmin,
    ]
