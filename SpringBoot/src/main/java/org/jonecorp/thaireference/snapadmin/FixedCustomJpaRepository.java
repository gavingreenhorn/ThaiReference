package org.jonecorp.thaireference.snapadmin;

import jakarta.persistence.EntityManager;
import jakarta.persistence.criteria.CriteriaBuilder;
import jakarta.persistence.criteria.CriteriaQuery;
import jakarta.persistence.criteria.Path;
import jakarta.persistence.criteria.Predicate;
import jakarta.persistence.criteria.Root;
import java.time.LocalDate;
import java.time.LocalDateTime;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
import java.util.stream.Collectors;
import tech.ailef.snapadmin.external.dbmapping.CustomJpaRepository;
import tech.ailef.snapadmin.external.dbmapping.DbObjectSchema;
import tech.ailef.snapadmin.external.dbmapping.fields.DbField;
import tech.ailef.snapadmin.external.dbmapping.fields.StringFieldType;
import tech.ailef.snapadmin.external.dbmapping.fields.TextFieldType;
import tech.ailef.snapadmin.external.dto.CompareOperator;
import tech.ailef.snapadmin.external.dto.QueryFilter;
import tech.ailef.snapadmin.external.exceptions.SnapAdminException;

/**
 * SnapAdmin 0.2.1 {@code search} always ORs {@code cast(pk as string) = q}. With a blank {@code q}
 * (pagination / page-size only) PostgreSQL rejects {@code integer = character varying}. Only apply
 * the PK clause when {@code q} has text.
 */
@SuppressWarnings({"rawtypes", "unchecked"})
public final class FixedCustomJpaRepository extends CustomJpaRepository {

    private final EntityManager entityManager;
    private final DbObjectSchema schema;

    public FixedCustomJpaRepository(DbObjectSchema schema, EntityManager entityManager) {
        super(schema, entityManager);
        this.entityManager = entityManager;
        this.schema = schema;
    }

    @Override
    public List search(String q, int page, int pageSize, String sortKey, String sortOrder, Set filters) {
        CriteriaBuilder cb = entityManager.getCriteriaBuilder();
        CriteriaQuery query = cb.createQuery(schema.getJavaClass());
        Root root = query.from(schema.getJavaClass());

        List<Predicate> finalPredicates = buildPredicates(q, filters, cb, root);
        query.select(root);

        boolean hasTextQuery = q != null && !q.isBlank();
        if (!finalPredicates.isEmpty() && hasTextQuery) {
            query.where(cb.or(
                    cb.and(finalPredicates.toArray(Predicate[]::new)),
                    cb.equal(root.get(schema.getPrimaryKey().getName()).as(String.class), q)));
        } else if (!finalPredicates.isEmpty()) {
            query.where(cb.and(finalPredicates.toArray(Predicate[]::new)));
        } else if (hasTextQuery) {
            query.where(cb.equal(root.get(schema.getPrimaryKey().getName()).as(String.class), q));
        }

        if (sortKey != null) {
            query.orderBy(
                    "DESC".equals(sortOrder) ? cb.desc(root.get(sortKey)) : cb.asc(root.get(sortKey)));
        }

        return entityManager
                .createQuery(query)
                .setMaxResults(pageSize)
                .setFirstResult((page - 1) * pageSize)
                .getResultList();
    }

    /** Same as SnapAdmin's private {@code buildPredicates}; kept here because that method is not visible. */
    private List<Predicate> buildPredicates(
            String q, Set queryFilters, CriteriaBuilder cb, Path root) {
        List<Predicate> finalPredicates = new ArrayList<>();

        List<DbField> stringFields = schema.getSortedFields().stream()
                .filter(f -> f.getType() instanceof StringFieldType || f.getType() instanceof TextFieldType)
                .collect(Collectors.toList());

        if (q != null && !q.isBlank()) {
            List<Predicate> queryPredicates = new ArrayList<>();
            for (DbField f : stringFields) {
                Path path = root.get(f.getJavaName());
                queryPredicates.add(cb.like(cb.lower(cb.toString(path)), "%" + q.toLowerCase() + "%"));
            }
            if (!queryPredicates.isEmpty()) {
                finalPredicates.add(cb.or(queryPredicates.toArray(Predicate[]::new)));
            }
        }

        if (queryFilters == null) {
            queryFilters = new HashSet<>();
        }
        for (QueryFilter filter : (Set<QueryFilter>) queryFilters) {
            CompareOperator op = filter.getOp();
            DbField dbField = filter.getField();
            String fieldName = dbField.getJavaName();
            String v = filter.getValue();

            Object value = null;
            if (!v.isBlank()) {
                try {
                    value = dbField.getType().parseValue(v);
                } catch (Exception e) {
                    throw new SnapAdminException(
                            "Invalid value `" + v + "` specified for field `" + dbField.getName() + "`");
                }
            }

            if (op == CompareOperator.STRING_EQ) {
                if (value == null) {
                    finalPredicates.add(cb.isNull(root.get(fieldName)));
                } else {
                    finalPredicates.add(
                            cb.equal(cb.lower(cb.toString(root.get(fieldName))), value.toString().toLowerCase()));
                }
            } else if (op == CompareOperator.CONTAINS) {
                if (value != null) {
                    finalPredicates.add(cb.like(
                            cb.lower(cb.toString(root.get(fieldName))),
                            "%" + value.toString().toLowerCase() + "%"));
                }
            } else if (op == CompareOperator.EQ) {
                finalPredicates.add(cb.equal(root.get(fieldName), value));
            } else if (op == CompareOperator.GT) {
                if (value != null) {
                    finalPredicates.add(cb.greaterThan(root.get(fieldName), value.toString()));
                }
            } else if (op == CompareOperator.LT) {
                if (value != null) {
                    finalPredicates.add(cb.lessThan(root.get(fieldName), value.toString()));
                }
            } else if (op == CompareOperator.AFTER) {
                if (value instanceof LocalDate) {
                    finalPredicates.add(cb.greaterThan(root.get(fieldName), (LocalDate) value));
                } else if (value instanceof LocalDateTime) {
                    finalPredicates.add(cb.greaterThan(root.get(fieldName), (LocalDateTime) value));
                }
            } else if (op == CompareOperator.BEFORE) {
                if (value instanceof LocalDate) {
                    finalPredicates.add(cb.lessThan(root.get(fieldName), (LocalDate) value));
                } else if (value instanceof LocalDateTime) {
                    finalPredicates.add(cb.lessThan(root.get(fieldName), (LocalDateTime) value));
                }
            }
        }
        return finalPredicates;
    }
}
