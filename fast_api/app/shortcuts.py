from sqlmodel import Relationship, Field, SQLModel

from app.models import thai_mapper_registry


class ThaiModel(SQLModel, registry=thai_mapper_registry):
    __abstract__ = True


def one_to_one(**kwargs):
    return Relationship(
        sa_relationship_kwargs={"uselist": False, **kwargs}
    )


def unique_field(**kwargs):
    return Field(
        sa_column_kwargs={"unique": True, **kwargs}
    )
