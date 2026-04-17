"""SQLAdmin ModelViews for Thai reference tables (SQLAlchemy orm_tables)."""
from sqladmin import ModelView

from app.models.orm_tables import (
    ClassOrm,
    CompositeNameOrm,
    ConsonantOrm,
    ConsonantSampleOrm,
    EndingOrm,
    FormalityOrm,
    GenderOrm,
    LexemeOrm,
    NumeralOrm,
    PluralityOrm,
    PronounOrm,
    PronounTypeBreakoutOrm,
    PronounTypeOrm,
    SoundTypeOrm,
    SoundOrm,
    SpecialSymbolOrm,
    ToneMarkOrm,
    TranscriptionOrm,
    VowelGraphemeOrm,
    VowelOrm,
    VowelSampleOrm,
    WordLexemeOrm,
    WordOrm,
)


class _ThaiModelView(ModelView):
    """Base admin view with create/edit/delete/details enabled."""
    can_create = True
    can_edit = True
    can_delete = True
    can_view_details = True


# Reference
class TranscriptionAdmin(_ThaiModelView, model=TranscriptionOrm):
    name = "Transcription"
    category = "Reference"
    column_list = [TranscriptionOrm.id, TranscriptionOrm.phonemic, TranscriptionOrm.paiboon, TranscriptionOrm.IPA]


class SoundTypeAdmin(_ThaiModelView, model=SoundTypeOrm):
    name = "Sound Type"
    category = "Reference"
    column_list = [SoundTypeOrm.id, SoundTypeOrm.name]


class LexemeAdmin(_ThaiModelView, model=LexemeOrm):
    name = "Lexeme"
    category = "Reference"
    column_list = [LexemeOrm.id, LexemeOrm.value]


class ClassAdmin(_ThaiModelView, model=ClassOrm):
    name = "Class"
    category = "Reference"
    column_list = [ClassOrm.id, ClassOrm.name]


class EndingAdmin(_ThaiModelView, model=EndingOrm):
    name = "Ending"
    category = "Reference"
    column_list = [EndingOrm.id, EndingOrm.name]


class GenderAdmin(_ThaiModelView, model=GenderOrm):
    name = "Gender"
    category = "Reference"
    column_list = [GenderOrm.id, GenderOrm.name]


class FormalityAdmin(_ThaiModelView, model=FormalityOrm):
    name = "Formality"
    category = "Reference"
    column_list = [FormalityOrm.id, FormalityOrm.name]


class PluralityAdmin(_ThaiModelView, model=PluralityOrm):
    name = "Plurality"
    category = "Reference"
    column_list = [PluralityOrm.id, PluralityOrm.name]


class PronounTypeAdmin(_ThaiModelView, model=PronounTypeOrm):
    name = "Pronoun Type"
    category = "Reference"
    column_list = [PronounTypeOrm.id, PronounTypeOrm.name]


# Content
class WordAdmin(_ThaiModelView, model=WordOrm):
    name = "Word"
    category = "Content"
    column_list = [WordOrm.id, WordOrm.transcription, WordOrm.value]


class WordLexemeAdmin(_ThaiModelView, model=WordLexemeOrm):
    name = "Word-Lexeme"
    category = "Associations"
    column_list = [WordLexemeOrm.id, WordLexemeOrm.word, WordLexemeOrm.lexeme]


class SoundAdmin(_ThaiModelView, model=SoundOrm):
    name = "Sound"
    category = "Content"
    column_list = [
        SoundOrm.id,
        SoundOrm.transcription,
        SoundOrm.mediapath,
        SoundOrm.initial_only,
        SoundOrm.consonant_sound,
        SoundOrm.sound_type,
    ]


class ConsonantAdmin(_ThaiModelView, model=ConsonantOrm):
    name = "Consonant"
    category = "Content"
    column_list = [
        ConsonantOrm.id,
        ConsonantOrm.grapheme,
        ConsonantOrm.phoneme,
        ConsonantOrm.soundtype,
        ConsonantOrm.class_,
        ConsonantOrm.ending,
        ConsonantOrm.transcription,
        ConsonantOrm.acrophonic,
        ConsonantOrm.mediapath,
        ConsonantOrm.initial,
        ConsonantOrm.final_sound_id,
    ]


class ConsonantSampleAdmin(_ThaiModelView, model=ConsonantSampleOrm):
    name = "Consonant Sample"
    category = "Associations"
    column_list = [ConsonantSampleOrm.id, ConsonantSampleOrm.consonant, ConsonantSampleOrm.word]


class VowelAdmin(_ThaiModelView, model=VowelOrm):
    name = "Vowel"
    category = "Content"
    column_list = [VowelOrm.id, VowelOrm.sound_id, VowelOrm.is_diphtong]


class VowelGraphemeAdmin(_ThaiModelView, model=VowelGraphemeOrm):
    name = "Vowel Grapheme"
    category = "Associations"
    column_list = [VowelGraphemeOrm.id, VowelGraphemeOrm.vowel_id, VowelGraphemeOrm.value, VowelGraphemeOrm.is_closed]


class VowelSampleAdmin(_ThaiModelView, model=VowelSampleOrm):
    name = "Vowel Sample"
    category = "Associations"
    column_list = [VowelSampleOrm.id, VowelSampleOrm.vowel, VowelSampleOrm.word]


class NumeralAdmin(_ThaiModelView, model=NumeralOrm):
    name = "Numeral"
    category = "Content"
    column_list = [NumeralOrm.id, NumeralOrm.word, NumeralOrm.grapheme]


class PronounAdmin(_ThaiModelView, model=PronounOrm):
    name = "Pronoun"
    category = "Content"
    column_list = [
        PronounOrm.id,
        PronounOrm.word,
        PronounOrm.gender,
        PronounOrm.formality,
        PronounOrm.plurality,
        PronounOrm.grammatical_person,
    ]


class PronounTypeBreakoutAdmin(_ThaiModelView, model=PronounTypeBreakoutOrm):
    name = "Pronoun Type Breakout"
    category = "Associations"
    column_list = [PronounTypeBreakoutOrm.id, PronounTypeBreakoutOrm.pronoun_id, PronounTypeBreakoutOrm.type_id]


class SpecialSymbolAdmin(_ThaiModelView, model=SpecialSymbolOrm):
    name = "Special Symbol"
    category = "Content"
    column_list = [SpecialSymbolOrm.id, SpecialSymbolOrm.grapheme, SpecialSymbolOrm.word_id]


class CompositeNameAdmin(_ThaiModelView, model=CompositeNameOrm):
    name = "Composite Name"
    category = "Content"
    column_list = [
        CompositeNameOrm.id,
        CompositeNameOrm.word1,
        CompositeNameOrm.word2,
        CompositeNameOrm.word3,
        CompositeNameOrm.word4,
        CompositeNameOrm.word5,
    ]


class ToneMarkAdmin(_ThaiModelView, model=ToneMarkOrm):
    name = "Tone Mark"
    category = "Content"
    column_list = [ToneMarkOrm.id, ToneMarkOrm.mark, ToneMarkOrm.name, ToneMarkOrm.mediapath]


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
        WordLexemeAdmin,
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
