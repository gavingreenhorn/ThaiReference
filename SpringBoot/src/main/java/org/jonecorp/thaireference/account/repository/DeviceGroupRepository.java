package org.jonecorp.thaireference.account.repository;

import org.jonecorp.thaireference.account.entity.DeviceGroup;
import java.util.Optional;
import org.springframework.data.jpa.repository.JpaRepository;

public interface DeviceGroupRepository extends JpaRepository<DeviceGroup, Long> {

    Optional<DeviceGroup> findByEmail(String email);
}
