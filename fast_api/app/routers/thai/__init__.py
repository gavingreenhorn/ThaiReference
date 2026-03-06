from fastapi import APIRouter

from .data import router as data_router

router = APIRouter(prefix="/thai")
router.include_router(data_router)
