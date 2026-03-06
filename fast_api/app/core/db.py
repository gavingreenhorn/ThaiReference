from typing import Annotated

from fastapi import Depends
from sqlalchemy import MetaData, create_engine
from sqlalchemy.orm import Session, registry

from app.core.config import settings

_url = settings.database_url
_connect_args = {"check_same_thread": False} if _url.startswith("sqlite") else {}
engine = create_engine(_url, echo=False, connect_args=_connect_args)

# Mapper registry used by SQLModel models (PostgreSQL path / Alembic).
# No schema specified here; migrations own schema details when needed.
thai_mapper_registry = registry(metadata=MetaData())


def get_session():
    with Session(engine) as session:
        yield session


SessionDep = Annotated[Session, Depends(get_session)]
