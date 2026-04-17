from typing import Annotated

from fastapi import Depends
from sqlalchemy import create_engine
from sqlalchemy.orm import Session

from app.core.config import settings

engine = create_engine(settings.dsn, echo=False)


def get_session():
    with Session(engine) as session:
        yield session


SessionDep = Annotated[Session, Depends(get_session)]
