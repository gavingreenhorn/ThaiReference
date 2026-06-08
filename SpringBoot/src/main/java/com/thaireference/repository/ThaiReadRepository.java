package com.thaireference.repository;

import com.thaireference.dto.SoundDto;
import com.thaireference.entity.Sound;
import java.util.List;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;

public interface ThaiReadRepository extends JpaRepository<Sound, Integer>, ThaiReadRepositoryCustom {

    @Query(
            """
            SELECT new com.thaireference.dto.SoundDto(
                s.id,
                s.value,
                COALESCE(st.name, ''),
                CASE WHEN s.consonantSound IS NOT NULL THEN true ELSE false END,
                s.initialOnly,
                s.mediapath)
            FROM Sound s
            LEFT JOIN SoundType st ON st.id = s.soundTypeId
            ORDER BY s.id
            """)
    List<SoundDto> findSounds();
}
