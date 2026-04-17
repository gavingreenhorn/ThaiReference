from sqlmodel import Relationship, Field, SQLModel


class ThaiModel(SQLModel):
    __abstract__ = True


def one_to_one(**kwargs):
    return Relationship(
        sa_relationship_kwargs={"uselist": False, **kwargs}
    )


def unique_field(**kwargs):
    return Field(
        sa_column_kwargs={"unique": True, **kwargs}
    )
