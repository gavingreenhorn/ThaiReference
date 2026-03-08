import os
from contextlib import asynccontextmanager

import redis.asyncio as redis
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi_admin.app import app as admin_app
from fastapi_admin.providers.login import UsernamePasswordProvider
from tortoise.contrib.fastapi import register_tortoise

from app.core.config import settings
from app.models.admin_model import Admin
from app.routers.admin import router as admin_router
from app.routers.thai import router as thai_router

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

login_provider = UsernamePasswordProvider(
    admin_model=Admin,
    login_logo_url="https://preview.tabler.io/static/logo.svg",
)


def _tortoise_url() -> str:
    url = settings.database_url
    if url.startswith("postgresql+psycopg2://"):
        return url.replace("postgresql+psycopg2://", "postgres://", 1)
    return url


@asynccontextmanager
async def lifespan(app: FastAPI):
    r = redis.from_url(settings.redis_url, decode_responses=True, encoding="utf8")
    await r.ping()
    await admin_app.configure(
        logo_url="https://preview.tabler.io/static/logo-white.svg",
        template_folders=[os.path.join(BASE_DIR, "templates")],
        providers=[login_provider],
        redis=r,
    )
    yield


app = FastAPI(title=settings.app_title, lifespan=lifespan)
app.mount("/admin", admin_app)

register_tortoise(
    app,
    config={
        "connections": {"default": _tortoise_url()},
        "apps": {
            "models": {
                "models": ["app.models.admin_model"],
                "default_connection": "default",
            }
        },
    },
    generate_schemas=True,
)

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
