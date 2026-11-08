package com.thaireference.repository;

import com.thaireference.dto.SoundDto;
import com.thaireference.dto.TranscriptionDto;
import com.thaireference.dto.VowelGraphemeDto;
import com.thaireference.dto.WordDto;
import com.thaireference.dto.ZodiacDto;

/*
 * A JPQL constructor projection (SELECT new Dto(...)) cannot fill a nested List, so any parent
 * that owns a list of words/graphemes/types is read with two queries and joined in Java by id:
 *
 *   1. a parent row that carries the parent's own fields plus its id   (the *Row records below),
 *   2. child rows that carry (parentId, child)                         (WordRow / GraphemeRow / PronounTypeRow).
 *
 * The repository groups the child rows by parentId and hands each parent its own children.
 * Parents whose DTO already exposes the join id (Cluster, MinimalSet) skip step 1 and project
 * straight into the DTO, so they have no row here.
 */

// --- parent rows: parent fields + the id its children are attached by ---

record ConsonantRow(
        Integer id,
        String grapheme,
        String phoneme,
        String consonantClass,
        String ending,
        TranscriptionDto transcription,
        SoundDto initialSound,
        SoundDto endingSound,
        String mediaUrl) {}

record VowelRow(Integer id, boolean isDiphtong, SoundDto sound, String mediaUrl) {}

record PronounRow(Integer id, WordDto name, int person, int plurality, String formality, String gender) {}

record GridPositionRow(Integer id, Integer row, Integer col, String tileStyle, String label) {}

record MarkRow(Integer id, String grapheme, boolean toneMark, String englishLabel) {}

record ClassRow(Integer id, String code, String englishLabel) {}

record HomophoneRow(Integer transcriptionId, TranscriptionDto transcription) {}

record DirectionRow(Integer id, String label, String lexeme) {}

record PositionHeadRow(Integer id, String lexeme, Integer sentenceId, String translation) {}

record MonthRow(
        Integer ordinal,
        WordDto word,
        String shortName,
        String englishName,
        ZodiacDto zodiac,
        Integer seasonId,
        Integer seasonOrdinal,
        String seasonEnglishLabel,
        String seasonGlyph) {}

// --- child rows: (parentId, child), grouped back onto the parent by parentId ---

record WordRow(Integer parentId, WordDto word) {}

record GraphemeRow(Integer vowelId, VowelGraphemeDto grapheme) {}

record PronounTypeRow(Integer pronounId, String type) {}

// --- color rows: flat data from ColorView, grouped by family in Java ---

record ColorRow(
        Integer colorId,
        Integer familyId,
        Integer compositeNameId,
        String hex,
        String englishLabel,
        Integer luminanceOrder) {}
