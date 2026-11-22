/*
 * SnapAdmin - An automatically generated CRUD admin UI for Spring Boot apps
 * Copyright (C) 2023 Ailef (http://ailef.tech)
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

package tech.ailef.snapadmin.external;

import java.lang.reflect.Field;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.ParameterizedType;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import java.util.Optional;
import java.util.Set;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.config.BeanDefinition;
import org.springframework.context.annotation.ClassPathScanningCandidateComponentProvider;
import org.springframework.core.type.filter.AnnotationTypeFilter;
import org.springframework.stereotype.Component;

import jakarta.annotation.PostConstruct;
import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.EntityManager;
import jakarta.persistence.EnumType;
import jakarta.persistence.Enumerated;
import jakarta.persistence.Id;
import jakarta.persistence.JoinColumn;
import jakarta.persistence.Lob;
import jakarta.persistence.ManyToMany;
import jakarta.persistence.ManyToOne;
import jakarta.persistence.OneToMany;
import jakarta.persistence.OneToOne;
import tech.ailef.snapadmin.external.annotations.Disable;
import tech.ailef.snapadmin.external.annotations.DisplayFormat;
import tech.ailef.snapadmin.external.dbmapping.CustomJpaRepository;
import tech.ailef.snapadmin.external.dbmapping.DbObjectSchema;
import tech.ailef.snapadmin.external.dbmapping.fields.DbField;
import tech.ailef.snapadmin.external.dbmapping.fields.DbFieldType;
import tech.ailef.snapadmin.external.dbmapping.fields.EnumFieldType;
import tech.ailef.snapadmin.external.dbmapping.fields.StringFieldType;
import tech.ailef.snapadmin.external.dbmapping.fields.TextFieldType;
import tech.ailef.snapadmin.external.dto.MappingError;
import tech.ailef.snapadmin.external.exceptions.SnapAdminException;
import tech.ailef.snapadmin.external.exceptions.SnapAdminNotFoundException;
import tech.ailef.snapadmin.external.exceptions.UnsupportedFieldTypeException;
import tech.ailef.snapadmin.external.misc.Utils;

/**
 * Maps configured JPA entities to SnapAdmin schemas.
 */
@Component
public class SnapAdmin {
    private static final Logger logger = LoggerFactory.getLogger(SnapAdmin.class.getName());

    private EntityManager entityManager;
    private List<DbObjectSchema> schemas = new ArrayList<>();
    private List<String> modelsPackage;
    private SnapAdminProperties properties;
    private boolean authenticated;
    private static final String VERSION = "0.2.0";

    public SnapAdmin(@Autowired EntityManager entityManager, @Autowired SnapAdminProperties properties) {
        this.modelsPackage = Arrays.stream(properties.getModelsPackage().split(",")).map(String::trim).toList();
        this.entityManager = entityManager;
        this.properties = properties;
    }

    @PostConstruct
    private void init() {
        ClassPathScanningCandidateComponentProvider provider = new ClassPathScanningCandidateComponentProvider(false);
        provider.addIncludeFilter(new AnnotationTypeFilter(Entity.class));

        logger.debug("Initializing SnapAdmin...");

        for (String currentPackage : modelsPackage) {
            logger.debug("Scanning package " + currentPackage);

            Set<BeanDefinition> beanDefs = provider.findCandidateComponents(currentPackage);
            logger.debug("Found " + beanDefs.size() + " candidate @Entity classes");

            for (BeanDefinition bd : beanDefs) {
                DbObjectSchema schema = processBeanDefinition(bd);
                if (schema != null) {
                    schemas.add(schema);
                }
            }

            logger.info("Scanned package '" + currentPackage + "'. Loaded " + beanDefs.size() + " schemas.");
        }

        boolean hasErrors = schemas.stream().flatMap(s -> s.getErrors().stream()).count() > 0;
        logger.info("SnapAdmin initialized. Loaded " + schemas.size()
                + " schemas from " + modelsPackage.size() + " packages" + (hasErrors ? " (with errors)" : ""));
        logger.info("SnapAdmin web interface at: http://YOUR_HOST:YOUR_PORT/" + properties.getBaseUrl());
    }

    public String getVersion() {
        return VERSION;
    }

    public List<DbObjectSchema> getSchemas() {
        return Collections.unmodifiableList(schemas);
    }

    public DbObjectSchema findSchemaByClassName(String className) {
        return schemas.stream().filter(s -> s.getClassName().equals(className)).findFirst().orElseThrow(() -> {
            return new SnapAdminNotFoundException("Schema " + className + " not found.");
        });
    }

    public DbObjectSchema findSchemaByTableName(String tableName) {
        return schemas.stream().filter(s -> s.getTableName().equals(tableName)).findFirst().orElseThrow(() -> {
            return new SnapAdminException("Schema " + tableName + " not found.");
        });
    }

    public DbObjectSchema findSchemaByClass(Class<?> klass) {
        return findSchemaByClassName(klass.getName());
    }

    public boolean isManagedClass(Class<?> klass) {
        Optional<DbObjectSchema> hasSchema =
                schemas.stream().filter(s -> s.getClassName().equals(klass.getName())).findFirst();
        return hasSchema.isPresent();
    }

    private DbObjectSchema processBeanDefinition(BeanDefinition bd) {
        String fullClassName = bd.getBeanClassName();

        try {
            Class<?> klass = Class.forName(fullClassName);

            Disable disabled = klass.getAnnotation(Disable.class);
            if (disabled != null) {
                return null;
            }

            DbObjectSchema schema = new DbObjectSchema(klass, this);
            CustomJpaRepository simpleJpaRepository = new CustomJpaRepository(schema, entityManager);
            schema.setJpaRepository(simpleJpaRepository);

            logger.debug("Processing class: " + klass + " - Table: " + schema.getTableName());

            List<Field> fields = new ArrayList<>(Arrays.asList(klass.getDeclaredFields()));
            if (fields.stream().noneMatch(field -> field.isAnnotationPresent(Id.class))) {
                for (Class<?> parent = klass.getSuperclass(); parent != null; parent = parent.getSuperclass()) {
                    Field inheritedId = Arrays.stream(parent.getDeclaredFields())
                            .filter(field -> field.isAnnotationPresent(Id.class))
                            .findFirst()
                            .orElse(null);
                    if (inheritedId != null) {
                        fields.add(0, inheritedId);
                        break;
                    }
                }
            }

            for (Field f : fields) {
                try {
                    DbField field = mapField(f, schema);
                    field.setSchema(schema);
                    schema.addField(field);
                } catch (UnsupportedFieldTypeException e) {
                    logger.warn("The class " + klass.getSimpleName() + " contains the field `"
                            + f.getName() + "` of type `" + f.getType().getSimpleName() + "`, which is not supported");
                    schema.addError(new MappingError(
                            "The class contains the field `" + f.getName() + "` of type `"
                                    + f.getType().getSimpleName() + "`, which is not supported"));
                }
            }

            logger.debug("Processed " + klass + ", extracted " + schema.getSortedFields().size() + " fields");
            return schema;
        } catch (ClassNotFoundException | IllegalArgumentException | SecurityException e) {
            throw new RuntimeException(e);
        }
    }

    private String determineFieldName(Field f) {
        Column[] columnAnnotations = f.getAnnotationsByType(Column.class);
        String fieldName = Utils.camelToSnake(f.getName());

        if (columnAnnotations.length != 0) {
            Column col = columnAnnotations[0];
            if (col.name() != null && !col.name().isBlank()) {
                fieldName = col.name();
            }
        }

        return fieldName;
    }

    private boolean determineNullable(Field f) {
        Column[] columnAnnotations = f.getAnnotationsByType(Column.class);

        boolean nullable = true;
        if (columnAnnotations.length != 0) {
            Column col = columnAnnotations[0];
            nullable = col.nullable();
        }

        return nullable;
    }

    private DbField mapField(Field f, DbObjectSchema schema) {
        logger.debug("Processing field " + f.getName());
        OneToMany oneToMany = f.getAnnotation(OneToMany.class);
        ManyToMany manyToMany = f.getAnnotation(ManyToMany.class);
        ManyToOne manyToOne = f.getAnnotation(ManyToOne.class);
        OneToOne oneToOne = f.getAnnotation(OneToOne.class);
        Lob lob = f.getAnnotation(Lob.class);

        String fieldName = determineFieldName(f);
        Class<?> connectedType = null;
        DbFieldType fieldType = null;
        try {
            Class<? extends DbFieldType> fieldTypeClass = DbFieldType.fromClass(f.getType());
            if (fieldTypeClass == StringFieldType.class && lob != null) {
                fieldTypeClass = TextFieldType.class;
            }

            if (fieldTypeClass != EnumFieldType.class) {
                try {
                    fieldType = fieldTypeClass.getConstructor().newInstance();
                } catch (InstantiationException | IllegalAccessException | IllegalArgumentException
                        | InvocationTargetException | NoSuchMethodException | SecurityException e) {
                    // Relationship fields are resolved below.
                }
            }
        } catch (SnapAdminException e) {
            // Relationship fields are resolved below.
        }

        if (manyToOne != null || oneToOne != null) {
            fieldName = mapRelationshipJoinColumn(f);
            fieldType = mapForeignKeyType(f.getType());
            connectedType = f.getType();
        }

        if (manyToMany != null || oneToMany != null) {
            ParameterizedType stringListType = (ParameterizedType) f.getGenericType();
            Class<?> targetEntityClass = (Class<?>) stringListType.getActualTypeArguments()[0];
            fieldType = mapForeignKeyType(targetEntityClass);
            connectedType = targetEntityClass;
        }

        if (fieldType == null) {
            Enumerated enumerated = f.getAnnotation(Enumerated.class);
            if (enumerated != null) {
                fieldType = new EnumFieldType(f.getType(), enumerated.value());
            }
        }

        if (fieldType == null) {
            throw new UnsupportedFieldTypeException("Unable to determine fieldType for " + f.getType());
        }

        DisplayFormat displayFormat = f.getAnnotation(DisplayFormat.class);
        DbField field = new DbField(f.getName(), fieldName, f, fieldType, schema,
                displayFormat != null ? displayFormat.format() : null);
        field.setConnectedType(connectedType);

        Id[] idAnnotations = f.getAnnotationsByType(Id.class);
        field.setPrimaryKey(idAnnotations.length != 0);
        field.setNullable(determineNullable(f));
        if (field.isPrimaryKey()) {
            field.setNullable(false);
        }

        return field;
    }

    private String mapRelationshipJoinColumn(Field f) {
        String joinColumnName = Utils.camelToSnake(f.getName()) + "_id";
        JoinColumn[] joinColumn = f.getAnnotationsByType(JoinColumn.class);
        if (joinColumn.length != 0) {
            joinColumnName = joinColumn[0].name();
        }
        return joinColumnName;
    }

    private DbFieldType mapForeignKeyType(Class<?> entityClass) {
        Field idField = null;
        for (Class<?> current = entityClass; current != null; current = current.getSuperclass()) {
            idField = Arrays.stream(current.getDeclaredFields())
                    .filter(field -> field.isAnnotationPresent(Id.class))
                    .findFirst()
                    .orElse(null);
            if (idField != null) {
                break;
            }
        }

        if (idField == null) {
            throw new SnapAdminException("Unable to find @Id field in Entity class " + entityClass);
        }

        try {
            return DbFieldType.fromClass(idField.getType()).getConstructor().newInstance();
        } catch (InstantiationException | IllegalAccessException | IllegalArgumentException
                | InvocationTargetException | NoSuchMethodException | SecurityException e) {
            throw new SnapAdminException(e);
        }
    }

    public boolean isAuthenticated() {
        return authenticated;
    }

    public void setAuthenticated(boolean authenticated) {
        this.authenticated = authenticated;
    }
}
