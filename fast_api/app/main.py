from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from sqladmin import Admin
from starlette.middleware.sessions import SessionMiddleware

from app.admin_auth import AdminAuth
from app.admin_views import get_admin_views
from app.core.config import settings
from app.core.db import engine
from app.routers.thai import router as thai_router

app = FastAPI(title=settings.app_title)

app.add_middleware(
    SessionMiddleware,
    secret_key=settings.admin_secret_key,
)
admin = Admin(
    app,
    engine,
    base_url="/admin",
    title="Thai Reference Admin",
    authentication_backend=AdminAuth(secret_key=settings.admin_secret_key),
)
for view in get_admin_views():
    admin.add_view(view)

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


@app.get("/health")
def health() -> dict[str, str]:
    return {"status": "ok"}
