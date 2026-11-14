package org.jonecorp.thaireference.account.repository;

import org.jonecorp.thaireference.account.entity.AccountToken;
import org.jonecorp.thaireference.account.entity.AccountTokenPurpose;
import java.util.Optional;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

public interface AccountTokenRepository extends JpaRepository<AccountToken, Long> {

    @Query("select t from AccountToken t where t.tokenHash = :hash and t.purpose = :purpose and t.usedAt is null")
    Optional<AccountToken> findFresh(
        @Param("hash") String tokenHash,
        @Param("purpose") AccountTokenPurpose purpose
    );

    void deleteByGroupId(Long groupId);
}
