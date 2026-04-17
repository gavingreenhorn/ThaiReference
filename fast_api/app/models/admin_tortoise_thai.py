from tortoise import fields
from tortoise.models import Model


class Transcription(Model):
    class Meta:
        table = "Transcription"

    id = fields.IntField(pk=True)
    phonemic = fields.CharField(max_length=25, null=True)
    paiboon = fields.CharField(max_length=25, null=True)
    IPA = fields.TextField(null=True)


class SoundType(Model):
    class Meta:
        table = "SoundType"

    id = fields.IntField(pk=True)
    name = fields.CharField(max_length=15)


class Lexeme(Model):
    class Meta:
        table = "Lexeme"

    id = fields.IntField(pk=True)
    value = fields.TextField(null=True)


class Class(Model):
    class Meta:
        table = "Class"

    id = fields.IntField(pk=True)
    name = fields.CharField(max_length=15)


class Ending(Model):
    class Meta:
        table = "Ending"

    id = fields.IntField(pk=True)
    name = fields.CharField(max_length=15)


class Gender(Model):
    class Meta:
        table = "Gender"

    id = fields.IntField(pk=True)
    name = fields.TextField(null=True)


class Formality(Model):
    class Meta:
        table = "Formality"

    id = fields.IntField(pk=True)
    name = fields.TextField(null=True)


class Plurality(Model):
    class Meta:
        table = "Plurality"

    id = fields.IntField(pk=True)
    name = fields.TextField(null=True)


class PronounType(Model):
    class Meta:
        table = "PronounType"

    id = fields.IntField(pk=True)
    name = fields.TextField(null=True)


class Word(Model):
    class Meta:
        table = "Word"

    id = fields.IntField(pk=True)
    transcription = fields.IntField(null=True)
    value = fields.CharField(max_length=25, null=True)


class WordLexeme(Model):
    class Meta:
        table = "WordLexeme"

    id = fields.IntField(pk=True)
    word = fields.IntField(null=True)
    lexeme = fields.IntField(null=True)


class Sound(Model):
    class Meta:
        table = "Sound"

    id = fields.IntField(pk=True)
    transcription = fields.IntField(null=True)
    mediapath = fields.TextField(null=True)
    initial_only = fields.BooleanField(default=False)
    consonant_sound = fields.IntField(null=True)
    sound_type = fields.IntField(null=True)


class Consonant(Model):
    class Meta:
        table = "Consonant"

    id = fields.IntField(pk=True)
    grapheme = fields.CharField(max_length=1)
    phoneme = fields.CharField(max_length=2)
    soundtype = fields.CharField(max_length=10)
    class_ = fields.CharField(max_length=10, source_field="class")
    ending = fields.CharField(max_length=5)
    transcription = fields.IntField(null=True)
    acrophonic = fields.IntField(null=True)
    mediapath = fields.TextField(null=True)
    initial = fields.IntField(null=True)
    final = fields.IntField(null=True, source_field="final")


class ConsonantSample(Model):
    class Meta:
        table = "ConsonantSample"

    id = fields.IntField(pk=True)
    consonant = fields.IntField(null=True)
    word = fields.IntField(null=True)


class VowelGrapheme(Model):
    class Meta:
        table = "VowelGrapheme"

    id = fields.IntField(pk=True)
    vowel_id = fields.IntField(null=True)
    value = fields.TextField(null=True)
    is_closed = fields.BooleanField(null=True)


class VowelSample(Model):
    class Meta:
        table = "VowelSample"

    id = fields.IntField(pk=True)
    vowel = fields.IntField(null=True)
    word = fields.IntField(null=True)


class Vowel(Model):
    class Meta:
        table = "Vowel"

    id = fields.IntField(pk=True)
    sound_id = fields.IntField(null=True)
    is_diphtong = fields.BooleanField(null=True)


class Numeral(Model):
    class Meta:
        table = "Numeral"

    id = fields.IntField(pk=True)
    word = fields.IntField(null=True)
    grapheme = fields.TextField(null=True)


class Pronoun(Model):
    class Meta:
        table = "Pronoun"

    id = fields.IntField(pk=True)
    word = fields.IntField(null=True)
    gender = fields.IntField(null=True)
    formality = fields.IntField(null=True)
    plurality = fields.IntField(null=True)
    grammatical_person = fields.IntField(null=True)


class PronounTypeBreakout(Model):
    class Meta:
        table = "PronounTypeBreakout"

    id = fields.IntField(pk=True)
    pronoun_id = fields.IntField(null=True)
    type_id = fields.IntField(null=True)


class SpecialSymbol(Model):
    class Meta:
        table = "SpecialSymbol"

    id = fields.IntField(pk=True)
    grapheme = fields.TextField(null=True)
    word_id = fields.IntField(null=True)


class CompositeName(Model):
    class Meta:
        table = "CompositeName"

    id = fields.IntField(pk=True)
    word1 = fields.IntField(null=True)
    word2 = fields.IntField(null=True)
    word3 = fields.IntField(null=True)
    word4 = fields.IntField(null=True)
    word5 = fields.IntField(null=True)


class ToneMark(Model):
    class Meta:
        table = "ToneMark"

    id = fields.IntField(pk=True)
    mark = fields.CharField(max_length=2)
    name = fields.IntField(null=True)
    mediapath = fields.TextField(null=True)
