from typing import Annotated

from fastapi import Body, Path, APIRouter
from pydantic import Field
from sqlmodel import select

from app.core.db import SessionDep
from app.models.thai_models import Word

router = APIRouter(prefix="/words")
PositiveInt = Annotated[int, Field(gt=0)]


@router.get("", response_model=list[Word])
async def get_all(
    session: SessionDep,
    skip: int = 0,
    limit: int = 0
):
    query = select(Word)
    if skip:
        query = query.offset(skip)
    if limit:
        query = query.limit(limit)
    return session.exec(query).all()


@router.get("/{word_id}", response_model=Word)
async def get_by_id(
    session: SessionDep,
    word_id: PositiveInt = Path(..., description="The ID of the word to retrieve")
):
    return session.get_one(Word, word_id)


@router.post("", response_model=Word, status_code=201)
async def create_word(
    session: SessionDep,
    word: Word = Body(..., description="The word to create")
):
    session.add(word)
    session.commit()
    session.refresh(word)
    return word
