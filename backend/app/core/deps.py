from typing import Annotated

from fastapi import Depends

from app.core.db import SessionDep
from app.core.repository import SQLAlchemyThaiRepository, ThaiRepository


def get_repository(session: SessionDep) -> ThaiRepository:
    return SQLAlchemyThaiRepository(session)


RepoDep = Annotated[ThaiRepository, Depends(get_repository)]
