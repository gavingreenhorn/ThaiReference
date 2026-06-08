package com.thaireference.repository;

import com.thaireference.dto.TranscriptionDto;
import com.thaireference.dto.VowelGraphemeDto;
import com.thaireference.dto.WordDto;
import com.thaireference.dto.SoundDto;

record IdWordRow(Integer parentId, WordDto word) {}

record IdStringRow(Integer parentId, String value) {}

record IdGraphemeRow(Integer vowelId, VowelGraphemeDto grapheme) {}

record ConsonantCoreRow(
        Integer id,
        String grapheme,
        String phoneme,
        String consonantClass,
        String ending,
        TranscriptionDto transcription,
        SoundDto initialSound,
        SoundDto endingSound,
        String mediaUrl) {}

record VowelCoreRow(Integer id, boolean isDiphtong, SoundDto sound, String mediaUrl) {}

record MarkCoreRow(Integer id, String grapheme, boolean toneMark, String englishLabel) {}

record PronounCoreRow(Integer id, WordDto name, int person, int plurality, String formality, String gender) {}

record ClusterCoreRow(Integer id, String clusterType, String subtype, String value) {}

record PositionCoreRow(Integer id, Integer rowIndex, Integer colIndex, String tileStyle, String label) {}

record HomographCoreRow(Integer spellingId, String spelling) {}

record HomophoneCoreRow(Integer transcriptionId, TranscriptionDto transcription) {}
