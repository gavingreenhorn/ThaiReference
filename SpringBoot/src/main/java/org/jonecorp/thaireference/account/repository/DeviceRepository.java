package org.jonecorp.thaireference.account.repository;

import org.jonecorp.thaireference.account.entity.Device;
import java.util.List;
import java.util.Optional;
import java.util.UUID;
import org.springframework.data.jpa.repository.JpaRepository;

public interface DeviceRepository extends JpaRepository<Device, Long> {

    Optional<Device> findByUuid(UUID uuid);

    List<Device> findByGroupIdOrderByLinkedAtAscIdAsc(Long groupId);

    long countByGroupId(Long groupId);
}
