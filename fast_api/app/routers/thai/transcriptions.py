from typing import Annotated

from fastapi import Body, Path, APIRouter
from pydantic import Field
from sqlmodel import select

from app.core.db import SessionDep
from app.models.thai_models import Transcription

router = APIRouter(prefix="/transcriptions")
PositiveInt = Annotated[int, Field(gt=0)]


@router.get("", response_model=list[Transcription])
async def get_all(
    session: SessionDep,
    skip: int = 0,
    limit: int = 0
):
    with session:
        query = select(Transcription)
        if skip:
            query = query.offset(skip)
        if limit:
            query = query.limit(limit)
        return session.exec(query).all()


@router.get("/{transcription_id}", response_model=Transcription)
async def get_by_id(
    session: SessionDep,
    transcription_id: PositiveInt = Path(..., description="The ID of the transcription to retrieve")
):
    with session:
        return session.get(Transcription, transcription_id)


@router.post("", response_model=Transcription, status_code=201)
async def create_transcription(
    session: SessionDep,
    transcription: Transcription = Body(..., description="The transcription to create")
):
    with session:
        session.add(transcription)
        session.commit()
        session.refresh(transcription)
        return transcription
