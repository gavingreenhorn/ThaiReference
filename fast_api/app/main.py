from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from app.core.config import settings
from app.routers.thai import router as thai_router
from app.routers.admin import router as admin_router


app = FastAPI(title=settings.app_title)

origins = settings.cors_origins_list
if origins:
    app.add_middleware(
        CORSMiddleware,
        allow_origins=origins,
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

app.include_router(thai_router)
app.include_router(admin_router)


@app.get("/health")
def health() -> dict[str, str]:
    return {"status": "ok"}

# uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload
